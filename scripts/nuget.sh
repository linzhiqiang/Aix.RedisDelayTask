set -ex

cd $(dirname $0)/../

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

mkdir -p $artifactsFolder


dotnet build ./src/Aix.RedisDelayTask/Aix.RedisDelayTask.csproj -c Release

dotnet pack ./src/Aix.RedisDelayTask/Aix.RedisDelayTask.csproj -c Release -o $artifactsFolder

dotnet nuget push ./$artifactsFolder/Aix.RedisDelayTask.*.nupkg -k $PRIVATE_NUGET_KEY -s https://www.nuget.org
