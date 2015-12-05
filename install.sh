#!/usr/bin/env bash
if [ "$OS" == "Windows_NT" ]; then
    # use .Net

    ./paket.bootstrapper.exe
    exit_code=$?
    if [ $exit_code -ne 0 ]; then
        exit $exit_code
    fi

    paket.exe restore
    exit_code=$?
    if [ $exit_code -ne 0 ]; then
        exit $exit_code
    fi

    packages/FAKE/tools/FAKE.exe $@ install.fsx
else
    # use mono

    mono paket.bootstrapper.exe
    exit_code=$?
    if [ $exit_code -ne 0 ]; then
        exit $exit_code
    fi

    mono paket.exe restore
    exit_code=$?
    if [ $exit_code -ne 0 ]; then
        exit $exit_code
    fi

    mono packages/FAKE/tools/FAKE.exe $@ --fsiargs -d:MONO install.fsx
fi

