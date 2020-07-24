using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using Akka.Util.Internal;
using Avro.Generic;
using DotNetty.Common.Utilities;
using Google.Protobuf.Collections;
using IdentityModel;
using Org.BouncyCastle.Utilities;
using SharpPulsar.Presto;
using SharpPulsar.Presto.Facebook.Type;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace SharpPulsar.Akka.Sql.Client
{

	public class Query
	{
        private readonly IStatementClient client;
		private readonly bool debug;

		public Query(IStatementClient client, bool debug)
		{
			this.client = requireNonNull(client, "client is null");
			this.debug = debug;
		}

		public virtual Optional<string> SetCatalog
		{
			get
			{
				return client.SetCatalog;
			}
		}

		public virtual Optional<string> SetSchema
		{
			get
			{
				return client.SetSchema;
			}
		}

		public virtual IDictionary<string, string> SetSessionProperties
		{
			get
			{
				return client.SetSessionProperties;
			}
		}

		public virtual ISet<string> ResetSessionProperties
		{
			get
			{
				return client.ResetSessionProperties;
			}
		}

		public virtual IDictionary<string, SelectedRole> SetRoles
		{
			get
			{
				return client.SetRoles;
			}
		}

		public virtual IDictionary<string, string> AddedPreparedStatements
		{
			get
			{
				return client.AddedPreparedStatements;
			}
		}

		public virtual ISet<string> DeallocatedPreparedStatements
		{
			get
			{
				return client.DeallocatedPreparedStatements;
			}
		}

		public virtual string StartedTransactionId
		{
			get
			{
				return client.StartedTransactionId;
			}
		}

		public virtual bool ClearTransactionId
		{
			get
			{
				return client.ClearTransactionId;
			}
		}

		public virtual bool renderOutput(PrintStream @out, CryptoRandom.OutputFormat outputFormat, bool interactive)
		{
			Thread clientThread = Thread.CurrentThread;
			SignalHandler oldHandler = Signal.handle(SIGINT, signal =>
			{
			if (ignoreUserInterrupt.get() || client.ClientAborted)
			{
				return;
			}
			client.close();
			clientThread.Interrupt();
			});
			try
			{
				return renderQueryOutput(@out, outputFormat, interactive);
			}
			finally
			{
				Signal.handle(SIGINT, oldHandler);
				Thread.interrupted(); // clear interrupt status
			}
		}

		private bool renderQueryOutput(PrintStream @out, CryptoRandom.OutputFormat outputFormat, bool interactive)
		{
			StatusPrinter statusPrinter = null;
			PrintStream errorChannel = interactive ? @out : System.err;
			WarningsPrinter warningsPrinter = new PrintStreamWarningsPrinter(System.err);

			if (interactive)
			{
				statusPrinter = new StatusPrinter(client, @out, debug);
				statusPrinter.printInitialStatusUpdates();
			}
			else
			{
				processInitialStatusUpdates(warningsPrinter);
			}

			// if running or finished
			if (client.Running || (client.Finished && client.finalStatusInfo().Error == null))
			{
				QueryStatusInfo results = client.Running ? client.currentStatusInfo() : client.finalStatusInfo();
				if (results.UpdateType != null)
				{
					renderUpdate(errorChannel, results);
				}
				else if (results.Columns == null)
				{
					errorChannel.printf("Query %s has no columns\n", results.Id);
					return false;
				}
				else
				{
					renderResults(@out, outputFormat, interactive, results.Columns);
				}
			}

			checkState(!client.Running);

			if (statusPrinter != null)
			{
				// Print all warnings at the end of the query
				(new PrintStreamWarningsPrinter(System.err)).print(client.finalStatusInfo().Warnings, true, true);
				statusPrinter.printFinalInfo();
			}
			else
			{
				// Print remaining warnings separated
				warningsPrinter.print(client.finalStatusInfo().Warnings, true, true);
			}

			if (client.ClientAborted)
			{
				errorChannel.println("Query aborted by user");
				return false;
			}
			if (client.ClientError)
			{
				errorChannel.println("Query is gone (server restarted?)");
				return false;
			}

			verify(client.Finished);
			if (client.finalStatusInfo().Error != null)
			{
				renderFailure(errorChannel);
				return false;
			}

			return true;
		}

		private void processInitialStatusUpdates(WarningsPrinter warningsPrinter)
		{
			while (client.Running && (client.currentData().Data == null))
			{
				warningsPrinter.print(client.currentStatusInfo().Warnings, true, false);
				client.advance();
			}
			IList<PrestoWarning> warnings;
			if (client.Running)
			{
				warnings = client.currentStatusInfo().Warnings;
			}
			else
			{
				warnings = client.finalStatusInfo().Warnings;
			}
			warningsPrinter.print(warnings, false, true);
		}

		private void renderUpdate(PrintStream @out, QueryStatusInfo results)
		{
			string status = results.UpdateType;
			if (results.UpdateCount != null)
			{
				long count = results.UpdateCount;
				status += format(": %s row%s", count, (count != 1) ? "s" : "");
			}
			@out.println(status);
			discardResults();
		}

		private void discardResults()
		{
			try
			{
					using (OutputHandler handler = new OutputHandler(new NullPrinter()))
					{
					handler.processRows(client);
					}
			}
			catch (IOException e)
			{
				throw new UncheckedIOException(e);
			}
		}

		private void renderResults(PrintStream @out, CryptoRandom.OutputFormat outputFormat, bool interactive, IList<Column> columns)
		{
			try
			{
				doRenderResults(@out, outputFormat, interactive, columns);
			}
			catch (QueryAbortedException)
			{
				System.Console.WriteLine("(query aborted by user)");
				client.close();
			}
			catch (IOException e)
			{
				throw new UncheckedIOException(e);
			}
		}
        private void doRenderResults(PrintStream @out, CryptoRandom.OutputFormat format, bool interactive, IList<Column> columns)
		{
			IList<string> fieldNames = Lists.transform(columns, Column.getName);
			if (interactive)
			{
				pageOutput(format, fieldNames);
			}
			else
			{
				sendOutput(@out, format, fieldNames);
			}
		}

        private void pageOutput(CryptoRandom.OutputFormat format, IList<string> fieldNames)
		{
			try
			{
					using (Pager pager = Pager.create(), ThreadInterruptor clientThread = new ThreadInterruptor(), Writer<> writer = createWriter(pager), OutputHandler handler = createOutputHandler(format, writer, fieldNames))
					{
					if (!pager.NullPager)
					{
						// ignore the user pressing ctrl-C while in the pager
						ignoreUserInterrupt.set(true);
						pager.FinishFuture.thenRun(() =>
						{
						ignoreUserInterrupt.set(false);
						client.close();
						clientThread.interrupt();
						});
					}
					handler.processRows(client);
					}
			}
			catch (Exception e) when (e is Exception || e is IOException)
			{
				if (client.ClientAborted && !(e is QueryAbortedException))
				{
					throw new QueryAbortedException(e);
				}
				throw e;
			}
		}
        private void sendOutput(PrintStream @out, CryptoRandom.OutputFormat format, IList<string> fieldNames)
		{
			using (OutputHandler handler = createOutputHandler(format, createWriter(@out), fieldNames))
			{
				handler.processRows(client);
			}
		}

		private static OutputHandler createOutputHandler(CryptoRandom.OutputFormat format, Writer writer, IList<string> fieldNames)
		{
			return new OutputHandler(createOutputPrinter(format, writer, fieldNames));
		}

		private static OutputPrinter createOutputPrinter(CryptoRandom.OutputFormat format, Writer writer, IList<string> fieldNames)
		{
			switch (format)
			{
				case CryptoRandom.OutputFormat.ALIGNED:
					return new AlignedTablePrinter(fieldNames, writer);
				case CryptoRandom.OutputFormat.VERTICAL:
					return new VerticalRecordPrinter(fieldNames, writer);
				case CryptoRandom.OutputFormat.CSV:
					return new CsvPrinter(fieldNames, writer, false);
				case CryptoRandom.OutputFormat.CSV_HEADER:
					return new CsvPrinter(fieldNames, writer, true);
				case CryptoRandom.OutputFormat.TSV:
					return new TsvPrinter(fieldNames, writer, false);
				case CryptoRandom.OutputFormat.TSV_HEADER:
					return new TsvPrinter(fieldNames, writer, true);
				case CryptoRandom.OutputFormat.NULL:
					return new NullPrinter();
			}
			throw new Exception(format + " not supported");
		}

		private static Writer createWriter(Stream @out)
		{
			return new StreamWriter(@out, UTF_8);
		}

		public virtual void Dispose()
		{
			client.close();
		}

		public virtual void renderFailure(PrintStream @out)
		{
			QueryStatusInfo results = client.finalStatusInfo();
			QueryError error = results.Error;
			checkState(error != null);

			@out.printf("Query %s failed: %s%n", results.Id, error.Message);
			if (debug && (error.FailureInfo != null))
			{
				error.FailureInfo.toException().printStackTrace(@out);
			}
			if (error.ErrorLocation != null)
			{
				renderErrorLocation(client.Query, error.ErrorLocation, @out);
			}
			@out.println();
		}

		private static void renderErrorLocation(string query, ErrorLocation location, PrintStream @out)
		{
			IList<string> lines = ImmutableList.copyOf(Splitter.on('\n').Split(query).GetEnumerator());

			string errorLine = lines[location.LineNumber - 1];
			string good = errorLine.Substring(0, location.ColumnNumber - 1);
			string bad = errorLine.Substring(location.ColumnNumber - 1);

			if ((location.LineNumber == lines.Count) && bad.Trim().Length == 0)
			{
				bad = " <EOF>";
			}

			if (REAL_TERMINAL)
			{
				Ansi ansi = Ansi.ansi();

				ansi.fg(Ansi.Color.CYAN);
				for (int i = 1; i < location.LineNumber; i++)
				{
					ansi.a(lines[i - 1]).newline();
				}
				ansi.a(good);

				ansi.fg(Ansi.Color.RED);
				ansi.a(bad).newline();
				for (int i = location.LineNumber; i < lines.Count; i++)
				{
					ansi.a(lines[i]).newline();
				}

				ansi.reset();
				@out.print(ansi);
			}
			else
			{
				string prefix = format("LINE %s: ", location.LineNumber);
				string padding = Strings.repeat(" ", prefix.Length + (location.ColumnNumber - 1));
				@out.println(prefix + errorLine);
				@out.println(padding + "^");
			}
		}

	}

}