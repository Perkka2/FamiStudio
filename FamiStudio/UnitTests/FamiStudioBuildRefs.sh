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
  ( set -x; "$EXE" "${f}.fms" unit-test "${f}_FamiStudioRef.txt" )
done

rm -f -- *_FamiStudioTest.txt
