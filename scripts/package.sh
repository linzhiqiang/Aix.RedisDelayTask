set -ex

cd $(dirname $0)/../

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

mkdir -p $artifactsFolder

dotnet restore ./Aix.RedisDelayTask.sln
dotnet build ./Aix.RedisDelayTask.sln -c Release


dotnet pack ./src/Aix.RedisDelayTask/Aix.RedisDelayTask.csproj -c Release -o $artifactsFolder
