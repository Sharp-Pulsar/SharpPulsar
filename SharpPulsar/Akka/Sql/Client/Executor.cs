using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Akka.Sql.Client
{
    public class Executor
    {
        private const string PROMPT_NAME = "presto";
		private static readonly Duration EXIT_DELAY = new Duration(3, SECONDS);

		private static readonly Pattern HISTORY_INDEX_PATTERN = Pattern.compile(@"!\d+");

		public HelpOption helpOption;

		public VersionOption versionOption = new VersionOption();

		public ClientOptions clientOptions = new ClientOptions();

		public virtual bool run()
		{
			ClientSession session = clientOptions.toClientSession();
			bool hasQuery = !isNullOrEmpty(clientOptions.execute);
			bool isFromFile = !isNullOrEmpty(clientOptions.file);

			if (!hasQuery && !isFromFile)
			{
				AnsiConsole.systemInstall();
			}

			initializeLogging(clientOptions.logLevelsFile);

			string query = clientOptions.execute;
			if (hasQuery)
			{
				query += ";";
			}

			if (isFromFile)
			{
				if (hasQuery)
				{
					throw new Exception("both --execute and --file specified");
				}
				try
				{
					query = Files.asCharSource(new File(clientOptions.file), UTF_8).read();
					hasQuery = true;
				}
				catch (IOException e)
				{
					throw new Exception(format("Error reading from file %s: %s", clientOptions.file, e.Message));
				}
			}

			// abort any running query if the CLI is terminated
			AtomicBoolean exiting = new AtomicBoolean();
			ThreadInterruptor interruptor = new ThreadInterruptor();
			System.Threading.CountdownEvent exited = new System.Threading.CountdownEvent(1);
			Runtime.Runtime.addShutdownHook(new Thread(() =>
			{
				exiting.set(true);
				interruptor.interrupt();
				awaitUninterruptibly(exited, EXIT_DELAY.toMillis(), MILLISECONDS);
			}));

			try
			{
				using (QueryRunner queryRunner = new QueryRunner(session, clientOptions.debug, Optional.ofNullable(clientOptions.socksProxy), Optional.ofNullable(clientOptions.httpProxy), Optional.ofNullable(clientOptions.keystorePath), Optional.ofNullable(clientOptions.keystorePassword), Optional.ofNullable(clientOptions.truststorePath), Optional.ofNullable(clientOptions.truststorePassword), Optional.ofNullable(clientOptions.accessToken), Optional.ofNullable(clientOptions.user), clientOptions.password ? Password : null, Optional.ofNullable(clientOptions.krb5Principal), Optional.ofNullable(clientOptions.krb5RemoteServiceName), Optional.ofNullable(clientOptions.krb5ConfigPath), Optional.ofNullable(clientOptions.krb5KeytabPath), Optional.ofNullable(clientOptions.krb5CredentialCachePath), !clientOptions.krb5DisableRemoteServiceHostnameCanonicalization))
				{
					if (hasQuery)
					{
						return executeCommand(queryRunner, query, clientOptions.outputFormat, clientOptions.ignoreErrors);
					}

					runConsole(queryRunner, exiting);
					return true;
				}
			}
			finally
			{
				exited.Signal();
				interruptor.Dispose();
			}
		}

		private string Password
		{
			get
			{
				checkState(!string.ReferenceEquals(clientOptions.user, null), "Username must be specified along with password");
				string defaultPassword = Environment.GetEnvironmentVariable("PRESTO_PASSWORD");
				if (!string.ReferenceEquals(defaultPassword, null))
				{
					return defaultPassword;
				}

				java.io.Console console = System.console();
				if (console == null)
				{
					throw new Exception("No console from which to read password");
				}
				char[] password = console.readPassword("Password: ");
				if (password != null)
				{
					return new string(password);
				}
				return "";
			}
		}

		private static void runConsole(QueryRunner queryRunner, AtomicBoolean exiting)
		{
			try
			{
				using (TableNameCompleter tableNameCompleter = new TableNameCompleter(queryRunner), LineReader reader = new LineReader(History, commandCompleter(), lowerCaseCommandCompleter(), tableNameCompleter))
					{
					tableNameCompleter.populateCache();
					StringBuilder buffer = new StringBuilder();
					while (!exiting.get())
					{
						// read a line of input from user
						string prompt = PROMPT_NAME;
						string schema = queryRunner.Session.Schema;
						if (!string.ReferenceEquals(schema, null))
						{
							prompt += ":" + schema;
						}
						if (buffer.Length > 0)
						{
							prompt = Strings.repeat(" ", prompt.Length - 1) + "-";
						}
						string commandPrompt = prompt + "> ";
						string line = reader.readLine(commandPrompt);

						// add buffer to history and clear on user interrupt
						if (reader.interrupted())
						{
							string partial = squeezeStatement(buffer.ToString());
							if (partial.Length > 0)
							{
								reader.History.add(partial);
							}
							buffer = new StringBuilder();
							continue;
						}

						// exit on EOF
						if (string.ReferenceEquals(line, null))
						{
							System.Console.WriteLine();
							return;
						}

						// check for special commands if this is the first line
						if (buffer.Length == 0)
						{
							string command = line.Trim();

							if (HISTORY_INDEX_PATTERN.matcher(command).matches())
							{
								int historyIndex = parseInt(command.Substring(1));
								History history = reader.History;
								if ((historyIndex <= 0) || (historyIndex > history.index()))
								{
									System.Console.Error.WriteLine("Command does not exist");
									continue;
								}
								line = history.get(historyIndex - 1).ToString();
								System.Console.WriteLine(commandPrompt + line);
							}

							if (command.EndsWith(";", StringComparison.Ordinal))
							{
								command = command.Substring(0, command.Length - 1).Trim();
							}

							switch (command.ToLower(ENGLISH))
							{
								case "exit":
								case "quit":
									return;
								case "history":
									foreach (History.Entry entry in reader.History)
									{
										//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
										System.Console.Write("%5d  %s%n", entry.index() + 1, entry.value());
									}
									continue;
								case "help":
									System.Console.WriteLine();
									System.Console.WriteLine(HelpText);
									continue;
							}
						}

						// not a command, add line to buffer
						buffer.Append(line).Append("\n");

						// execute any complete statements
						string sql = buffer.ToString();
						StatementSplitter splitter = new StatementSplitter(sql, ImmutableSet.of(";", @"\G"));
						foreach (Statement split in splitter.CompleteStatements)
						{
							OutputFormat outputFormat = OutputFormat.ALIGNED;
							if (split.terminator().Equals(@"\G"))
							{
								outputFormat = OutputFormat.VERTICAL;
							}

							process(queryRunner, split.statement(), outputFormat, tableNameCompleter.populateCache, true);
							reader.History.add(squeezeStatement(split.statement()) + split.terminator());
						}

						// replace buffer with trailing partial statement
						buffer = new StringBuilder();
						string partial = splitter.PartialStatement;
						if (partial.Length > 0)
						{
							buffer.Append(partial).Append('\n');
						}
					}
				}
			}
			catch (IOException e)
			{
				System.Console.Error.WriteLine("Readline error: " + e.Message);
			}
		}

		private static bool executeCommand(QueryRunner queryRunner, string query, OutputFormat outputFormat, bool ignoreErrors)
		{
			bool success = true;
			StatementSplitter splitter = new StatementSplitter(query);
			foreach (Statement split in splitter.CompleteStatements)
			{
				if (!isEmptyStatement(split.statement()))
				{
					if (!process(queryRunner, split.statement(), outputFormat, () =>
					{

					}, false))
					{
						if (!ignoreErrors)
						{
							return false;
						}
						success = false;
					}
				}
			}
			if (!isEmptyStatement(splitter.PartialStatement))
			{
				System.Console.Error.WriteLine("Non-terminated statement: " + splitter.PartialStatement);
				return false;
			}
			return success;
		}

		private static bool process(QueryRunner queryRunner, string sql, OutputFormat outputFormat, ThreadStart schemaChanged, bool interactive)
		{
			string finalSql;
			try
			{
				finalSql = preprocessQuery(Optional.ofNullable(queryRunner.Session.Catalog), Optional.ofNullable(queryRunner.Session.Schema), sql);
			}
			catch (QueryPreprocessorException e)
			{
				System.Console.Error.WriteLine(e.Message);
				if (queryRunner.Debug)
				{
					System.Console.WriteLine(e.ToString());
					System.Console.Write(e.StackTrace);
				}
				return false;
			}

			try
			{
				using (Query query = queryRunner.startQuery(finalSql))
				{
					bool success = query.renderOutput(System.out, outputFormat, interactive);

					ClientSession session = queryRunner.Session;

					// update catalog and schema if present
					if (query.SetCatalog.Present || query.SetSchema.Present)
					{
						session = ClientSession.builder(session).withCatalog(query.SetCatalog.orElse(session.Catalog)).withSchema(query.SetSchema.orElse(session.Schema)).build();
						schemaChanged.run();
					}

					// update transaction ID if necessary
					if (query.ClearTransactionId)
					{
						session = stripTransactionId(session);
					}

					ClientSession.Builder builder = ClientSession.builder(session);

					if (!string.ReferenceEquals(query.StartedTransactionId, null))
					{
						builder = builder.withTransactionId(query.StartedTransactionId);
					}

					// update session properties if present
					if (query.SetSessionProperties.Count > 0 || query.ResetSessionProperties.Count > 0)
					{
						IDictionary<string, string> sessionProperties = new Dictionary<string, string>(session.Properties);
						//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
						sessionProperties.putAll(query.SetSessionProperties);
						//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
						sessionProperties.Keys.removeAll(query.ResetSessionProperties);
						builder = builder.withProperties(sessionProperties);
					}

					// update session roles
					if (query.SetRoles.Count > 0)
					{
						IDictionary<string, SelectedRole> roles = new Dictionary<string, SelectedRole>(session.Roles);
						//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
						roles.putAll(query.SetRoles);
						builder = builder.withRoles(roles);
					}

					// update prepared statements if present
					if (query.AddedPreparedStatements.Count > 0 || query.DeallocatedPreparedStatements.Count > 0)
					{
						IDictionary<string, string> preparedStatements = new Dictionary<string, string>(session.PreparedStatements);
						//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
						preparedStatements.putAll(query.AddedPreparedStatements);
						//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
						preparedStatements.Keys.removeAll(query.DeallocatedPreparedStatements);
						builder = builder.withPreparedStatements(preparedStatements);
					}

					session = builder.build();
					queryRunner.Session = session;

					return success;
				}
			}
			catch (Exception e)
			{
				System.Console.Error.WriteLine("Error running command: " + e.Message);
				if (queryRunner.Debug)
				{
					System.Console.WriteLine(e.ToString());
					System.Console.Write(e.StackTrace);
				}
				return false;
			}
		}

		private static MemoryHistory History
		{
			get
			{
				string historyFilePath = Environment.GetEnvironmentVariable("PRESTO_HISTORY_FILE");
				File historyFile;
				if (isNullOrEmpty(historyFilePath))
				{
					historyFile = new File(UserHome, ".presto_history");
				}
				else
				{
					historyFile = new File(historyFilePath);
				}
				return getHistory(historyFile);
			}
		}

        internal static MemoryHistory getHistory(File historyFile)
		{
			MemoryHistory history;
			try
			{
				//  try creating the history file and its parents to check
				// whether the directory tree is readable/writeable
				createParentDirs(historyFile.ParentFile);
				historyFile.createNewFile();
				history = new FileHistory(historyFile);
				history.MaxSize = 10000;
			}
			catch (IOException e)
			{
				System.err.printf("WARNING: Failed to load history file (%s): %s. " + "History will not be available during this session.%n", historyFile, e.Message);
				history = new MemoryHistory();
			}
			history.AutoTrim = true;
			return history;
		}

		private static void initializeLogging(string logLevelsFile)
		{
			// unhook out and err while initializing logging or logger will print to them
			PrintStream @out = System.out;
			PrintStream err = System.err;

			try
			{
				LoggingConfiguration config = new LoggingConfiguration();

				if (string.ReferenceEquals(logLevelsFile, null))
				{
					System.Out = new PrintStream(nullOutputStream());
					System.Err = new PrintStream(nullOutputStream());

					config.ConsoleEnabled = false;
				}
				else
				{
					config.LevelsFile = logLevelsFile;
				}

				Logging logging = Logging.initialize();
				logging.configure(config);
			}
			finally
			{
				System.Out = @out;
				System.Err = err;
			}
		}
	}
}
