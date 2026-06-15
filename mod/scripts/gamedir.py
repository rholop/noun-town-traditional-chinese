"""Shared helper for locating the Unity *_Data directory.

The demo's data folder is "Noun Town Language Learning Demo_Data"; the full
game's will be named differently (e.g. "Noun Town Language Learning_Data").
Locating it by globbing for "*_Data" lets the build scripts run unmodified
against either install.
"""
import glob
import os


def find_streaming_assets(game_dir):
    matches = glob.glob(os.path.join(game_dir, "*_Data"))
    if len(matches) != 1:
        raise RuntimeError(
            f"expected exactly one *_Data directory in {game_dir!r}, found {matches}"
        )
    return os.path.join(matches[0], "StreamingAssets", "windows")
