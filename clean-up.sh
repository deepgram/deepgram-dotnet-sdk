#!/bin/bash

# set -o errexit
set -o nounset
set -o pipefail
set -o xtrace

rm -rf ./.vs
rm -rf ./dist

# delete all compile actifacts
find ./ -type d -iname obj -print0 | xargs -0 rm -rf
find ./ -type d -iname bin -print0 | xargs -0 rm -rf