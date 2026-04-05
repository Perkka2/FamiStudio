#!/usr/bin/env bash
set -euo pipefail

export NES_INCLUDE=..
export WINEDEBUG=-all

count=0

compile_rom_permutation() {
  echo "==========================="
  echo "Comparing with definitions:"
  echo "==========================="

  : > test_defs.inc

  rnd=$((RANDOM % 3))
  if (( rnd == 0 )); then
    echo "FAMISTUDIO_CFG_NTSC_SUPPORT=1" >> test_defs.inc
  elif (( rnd == 1 )); then
    echo "FAMISTUDIO_CFG_PAL_SUPPORT=1" >> test_defs.inc
  else
    echo "FAMISTUDIO_CFG_NTSC_SUPPORT=1" >> test_defs.inc
    echo "FAMISTUDIO_CFG_PAL_SUPPORT=1" >> test_defs.inc
  fi

  rnd=$((RANDOM % 2))
  if (( rnd == 1 )); then
    echo "FAMISTUDIO_CFG_SFX_SUPPORT=1" >> test_defs.inc
    rnd=$((RANDOM % 4 + 1))
    echo "FAMISTUDIO_CFG_SFX_STREAMS=$rnd" >> test_defs.inc
  fi

  rnd=$((RANDOM % 2))
  if (( rnd == 1 )); then
    echo "FAMISTUDIO_CFG_SMOOTH_VIBRATO=1" >> test_defs.inc
  fi

  rnd=$((RANDOM % 3))
  if (( rnd == 1 )); then
    echo "FAMISTUDIO_CFG_DPCM_SUPPORT=1" >> test_defs.inc

    rnd=$((RANDOM % 2))
    if (( rnd == 1 )); then
      echo "FAMISTUDIO_USE_DELTA_COUNTER=1" >> test_defs.inc
    fi

    rnd=$((RANDOM % 2))
    if (( rnd == 1 )); then
      echo "FAMISTUDIO_USE_DPCM_BANKSWITCHING=1" >> test_defs.inc
    fi

    rnd=$((RANDOM % 2))
    if (( rnd == 1 )); then
      echo "FAMISTUDIO_USE_DPCM_EXTENDED_RANGE=1" >> test_defs.inc
    fi
  fi

  rnd=$((RANDOM % 8))
  if (( rnd == 0 )); then
    echo "FAMISTUDIO_EXP_VRC6=1" >> test_defs.inc
  elif (( rnd == 1 )); then
    echo "FAMISTUDIO_EXP_VRC7=1" >> test_defs.inc
  elif (( rnd == 2 )); then
    echo "FAMISTUDIO_EXP_MMC5=1" >> test_defs.inc
  elif (( rnd == 3 )); then
    echo "FAMISTUDIO_EXP_S5B=1" >> test_defs.inc
  elif (( rnd == 4 )); then
    echo "FAMISTUDIO_EXP_FDS=1" >> test_defs.inc
    rnd=$((RANDOM % 2))
    if (( rnd == 1 )); then
      echo "FAMISTUDIO_USE_FDS_AUTOMOD=1" >> test_defs.inc
    fi
  elif (( rnd == 5 )); then
    echo "FAMISTUDIO_EXP_N163=1" >> test_defs.inc
    rnd=$((RANDOM % 8 + 1))
    echo "FAMISTUDIO_EXP_N163_CHN_CNT=$rnd" >> test_defs.inc
  elif (( rnd == 6 )); then
    echo "FAMISTUDIO_EXP_EPSM=1" >> test_defs.inc

    rnd=$((RANDOM % 4))
    echo "FAMISTUDIO_EXP_EPSM_SSG_CHN_CNT=$rnd" >> test_defs.inc

    rnd=$((RANDOM % 7))
    echo "FAMISTUDIO_EXP_EPSM_FM_CHN_CNT=$rnd" >> test_defs.inc

    rnd=$((RANDOM % 2))
    echo "FAMISTUDIO_EXP_EPSM_RHYTHM_CHN1_ENABLE=$rnd" >> test_defs.inc
    rnd=$((RANDOM % 2))
    echo "FAMISTUDIO_EXP_EPSM_RHYTHM_CHN2_ENABLE=$rnd" >> test_defs.inc
    rnd=$((RANDOM % 2))
    echo "FAMISTUDIO_EXP_EPSM_RHYTHM_CHN3_ENABLE=$rnd" >> test_defs.inc
    rnd=$((RANDOM % 2))
    echo "FAMISTUDIO_EXP_EPSM_RHYTHM_CHN4_ENABLE=$rnd" >> test_defs.inc
    rnd=$((RANDOM % 2))
    echo "FAMISTUDIO_EXP_EPSM_RHYTHM_CHN5_ENABLE=$rnd" >> test_defs.inc
    rnd=$((RANDOM % 2))
    echo "FAMISTUDIO_EXP_EPSM_RHYTHM_CHN6_ENABLE=$rnd" >> test_defs.inc
  else
    echo "FAMISTUDIO_EXP_RAINBOW=1" >> test_defs.inc
  fi

  rnd=$((RANDOM % 2))
  if (( rnd == 1 )); then
    echo "FAMISTUDIO_USE_FAMITRACKER_TEMPO=1" >> test_defs.inc
    rnd=$((RANDOM % 2))
    if (( rnd == 1 )); then
      echo "FAMISTUDIO_USE_FAMITRACKER_DELAYED_NOTES_OR_CUTS=1" >> test_defs.inc
    fi
  fi

  rnd=$((RANDOM % 2))
  if (( rnd == 1 )); then
    echo "FAMISTUDIO_USE_VOLUME_TRACK=1" >> test_defs.inc
    rnd=$((RANDOM % 2))
    if (( rnd == 1 )); then
      echo "FAMISTUDIO_USE_VOLUME_SLIDES=1" >> test_defs.inc
    fi
  fi

  rnd=$((RANDOM % 2))
  if (( rnd == 1 )); then
    echo "FAMISTUDIO_USE_PITCH_TRACK=1" >> test_defs.inc
  fi

  rnd=$((RANDOM % 2))
  if (( rnd == 1 )); then
    echo "FAMISTUDIO_USE_RELEASE_NOTES=1" >> test_defs.inc
  fi

  rnd=$((RANDOM % 2))
  if (( rnd == 1 )); then
    echo "FAMISTUDIO_USE_SLIDE_NOTES=1" >> test_defs.inc
    rnd=$((RANDOM % 2))
    if (( rnd == 1 )); then
      echo "FAMISTUDIO_USE_NOISE_SLIDE_NOTES=1" >> test_defs.inc
    fi
  fi

  rnd=$((RANDOM % 2))
  if (( rnd == 1 )); then
    echo "FAMISTUDIO_USE_VIBRATO=1" >> test_defs.inc
  fi

  rnd=$((RANDOM % 2))
  if (( rnd == 1 )); then
    echo "FAMISTUDIO_USE_ARPEGGIO=1" >> test_defs.inc
  fi

  rnd=$((RANDOM % 2))
  if (( rnd == 1 )); then
    echo "FAMISTUDIO_USE_DUTYCYCLE_EFFECT=1" >> test_defs.inc
  fi

  rnd=$((RANDOM % 2))
  if (( rnd == 1 )); then
    echo "FAMISTUDIO_USE_PHASE_RESET=1" >> test_defs.inc
  fi

  rnd=$((RANDOM % 2))
  if (( rnd == 1 )); then
    echo "FAMISTUDIO_USE_INSTRUMENT_EXTENDED_RANGE=1" >> test_defs.inc
  fi

  cat test_defs.inc

  cmd='..\..\Tools\ca65.exe test_ca65.s -g -o test_ca65.o && '
  cmd+='..\..\Tools\ld65.exe -C test_ca65.cfg -o test_ca65.nes test_ca65.o --mapfile test_ca65.map --dbgfile test_ca65.dbg && '
  cmd+='pushd .. && '
  cmd+='..\Tools\asm6_fixed.exe UnitTests\test_asm6.asm UnitTests\test_asm6.nes && '
  cmd+='popd && '
  cmd+='..\..\Tools\NESASM3.exe test_nesasm.asm && '
  cmd+='..\..\Tools\sdas6500.exe -pogn -I. -I.. -y -s -l test_sdas.o test_sdas.s && '
  cmd+='..\..\Tools\sdld6808.exe -n -i -j -y -w -u -w -b _ZP=0x0000 -b _BSS=0x300 -b _CODE=0x8000 test_sdas.ihx test_sdas.o && '
  cmd+='..\..\Tools\makebin.exe -s 73728 -o 32752 test_sdas.ihx test_sdas.nes'

  wine cmd /c "$cmd" || return 1 # NES_INCLUDE is needed for NESASM3 to find the notetables.

  cmp -s test_ca65.nes test_asm6.nes   || return 1
  cmp -s test_ca65.nes test_nesasm.nes || return 1
  cmp -s test_ca65.nes test_sdas.nes   || return 1

  rm -f -- *.o *.fns *.map *.dbg *.lst *.rst *.ihx
}

while (( count < 1000 )); do
  ((count+=1))
  if ! compile_rom_permutation; then
    echo
    echo "Error! ROMs are NOT identical with these definitions!"
    echo
    cat test_defs.inc
    exit 1
  fi
done

echo
echo "All ROMs are identical!"
