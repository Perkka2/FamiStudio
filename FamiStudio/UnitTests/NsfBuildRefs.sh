#!/usr/bin/env bash
set -euo pipefail
EXE="../bin/Release/net8.0/FamiStudio"

nsf_tests=(
  TestBase
  TestFDS
  TestMMC5
  TestN163
  TestS5B
  TestVRC6
  TestVRC7
  TestFamiTrackerTempo
  TestEPSM
  TestMulti
  TestMultiEPSM
)

nsf_default_import_tests=(
  TestBase
  TestFDS
  TestMMC5
  TestN163
  TestS5B
  TestVRC6
  TestVRC7
  TestFamiTrackerTempo
)

to_crlf() {
  sed -i.bak 's/\r\?$/\r/' "$1" && rm -f "$1.bak"
}

for f in "${nsf_tests[@]}"; do
  ( set -x; "$EXE" "${f}.fms" nsf-export "${f}.nsf" )
  echo
done

for f in "${nsf_default_import_tests[@]}"; do
  ( set -x; "$EXE" "${f}.nsf" famistudio-txt-export "${f}_NsfRef.txt" -nsf-import-pattern-length:160 -famistudio-txt-bare )
  to_crlf "${f}_NsfRef.txt"
done

( set -x; "$EXE" TestEPSM.nsf famistudio-txt-export TestEPSM_NsfRef.txt -nsf-import-pattern-length:160 -nsf-import-duration:170 -famistudio-txt-bare )
( set -x; "$EXE" TestMulti.nsf famistudio-txt-export TestMulti_NsfRef.txt -nsf-import-pattern-length:160 -nsf-import-duration:260 -famistudio-txt-bare )
( set -x; "$EXE" TestMultiEPSM.nsf famistudio-txt-export TestMultiEPSM_NsfRef.txt -nsf-import-pattern-length:160 -nsf-import-duration:380 -famistudio-txt-bare )

for f in TestEPSM TestMulti TestMultiEPSM; do
  to_crlf "${f}_NsfRef.txt"
done

rm -f -- *_NsfTest.txt
rm -f -- *.nsf
