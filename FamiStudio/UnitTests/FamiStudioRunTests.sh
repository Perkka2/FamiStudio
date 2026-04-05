#!/usr/bin/env bash
set -euo pipefail
EXE="../bin/Release/net8.0/FamiStudio"

tests=(
  TestBase
  TestFDS
  TestMMC5
  TestN163
  TestS5B
  TestVRC6
  TestVRC7
  TestEPSM
  TestMulti
  TestFamiTrackerTempo
)

for f in "${tests[@]}"; do
  ( set -x; "$EXE" "${f}.fms" unit-test "${f}_FamiStudioTest.txt" )
done

for f in "${tests[@]}"; do
  ( set -x; cmp -s "${f}_FamiStudioTest.txt" "${f}_FamiStudioRef.txt" ) || {
    echo
    echo "FamiStudio unit tests failed!"
    exit 1
  }
done

rm -f -- *_FamiStudioTest.txt

echo
echo "FamiStudio unit tests passed!"
