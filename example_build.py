import shutil
import subprocess
import sys
from pathlib import Path

# Input White Knuckle Directory Path here
GAME_FOLDER = Path("ExampleFolder\\SteamLibrary\\steamapps\\common\\White Knuckle")

if not GAME_FOLDER.exists():
    print(f"Unable to locate game folder: {GAME_FOLDER!s}")
    print("Please edit \"GAME_FOLDER\" variable in \"build.py\" to proceed.")
    sys.exit(1)

PROJECT_FOLDER = Path(__file__).parent
PLUGIN_NAME = Path("mimimi-turret_wk-localization-loader")
BINARY_NAME = Path("WKLocalizationLoader.dll")
BINARY_FOLDER = PROJECT_FOLDER.joinpath("bin\\Debug\\net472")
BINARY_PATH = BINARY_FOLDER.joinpath(BINARY_NAME)
PLUGINS_FOLDER = GAME_FOLDER.joinpath("BepInEx\\plugins")
TARGET_FOLDER = PLUGINS_FOLDER.joinpath(PLUGIN_NAME)
TARGET_PATH = TARGET_FOLDER.joinpath(BINARY_NAME)

command = [
    "dotnet",
    "build",
    f"-p:WhiteKnuckleDir={str(Path(GAME_FOLDER))}"
]
result = subprocess.run(command, text=True, cwd=PROJECT_FOLDER)
if result.returncode == 0 and BINARY_PATH.exists() and TARGET_FOLDER.exists():
    shutil.copy2(BINARY_PATH, TARGET_PATH)
    print("Copied binary to plugin's folder")
sys.exit(result.returncode)

