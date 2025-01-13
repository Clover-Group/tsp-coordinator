#!/usr/bin/env zsh
dotnet test -p:CollectCoverage=true -p:CoverletOutputFormat=cobertura
reportgenerator -reports:"TSPCoordinator.Tests/*.xml" -targetdir:"reports" --reporttypes: "Html"