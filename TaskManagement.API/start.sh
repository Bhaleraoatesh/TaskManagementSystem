#!/bin/sh
dotnet restore
dotnet build
dotnet run --urls=http://0.0.0.0:${PORT}
