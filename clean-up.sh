#!/bin/bash

# set -o errexit
set -o nounset
set -o pipefail
set -o xtrace

rm -rf ./.vs
rm -rf ./dist
rm -rf ./Deepgram/obj
rm -rf ./Deepgram/bin

# Deepgram.Tests
rm -rf ./Deepgram.Tests/bin
rm -rf ./Deepgram.Tests/obj

# Deepgram.Microphone
rm -rf ./Deepgram.Microphone/bin
rm -rf ./Deepgram.Microphone/obj