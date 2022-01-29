FROM mcr.microsoft.com/dotnet/sdk:6.0 
ENV test_file="Tests/SharpPulsar.Test/SharpPulsar.Test.csproj"
ENV test_filter=""
ENV run_count=0
RUN mkdir sharppulsar
COPY . ./sharppulsar
RUN ls
WORKDIR /sharppulsar
CMD ["/bin/bash", "-c", "x=1; c=0; while [ $x -le 1 ] && [ $c -le ${run_count} ]; do dotnet test ${test_file} ${test_filter} --framework net6.0 --logger trx --results-directory /var/TestResults; c=$(( $c + 1 )); if [ $? -eq 0 ]; then x=1; else x=0; fi;  done"]