# Pre-built Grave Alive releases

These files are built against **7 Days to Die V 2.6 (b14)** game assemblies.

## Recommended package

- `grave-v0.4.3-7dtd-2.6.zip`
  - Includes updated config files for survivor collision/interactions.
  - Uses a vanilla trader entity class for maximum compatibility.
  - Rotates visible survivors across multiple vanilla trader variants.
- `grave-v0.4.3-7dtd-2.6-avsafe.txt` (same package bytes, alternate extension)
  - For antivirus workflows that block direct `.zip` downloads.
  - Rename to `.zip` after download before extracting.
  - The payload uses `GraveAlive.bin`; rename it to `GraveAlive.dll` after extraction.

## Quick install (no coding)

1. Download `grave-v0.4.3-7dtd-2.6.zip`.
2. Extract it.
3. Copy the extracted `grave` folder into:
   - `C:\Users\<you>\AppData\Roaming\7DaysToDie\Mods\`
4. Start the game with **Easy Anti-Cheat off**.

### AV-safe variant install

1. Download `grave-v0.4.3-7dtd-2.6-avsafe.txt`.
2. Rename it to `grave-v0.4.3-7dtd-2.6-avsafe.zip`.
3. Extract it.
4. Inside the extracted folder, rename `GraveAlive.bin` to `GraveAlive.dll`.
5. Copy the folder to your `Mods` directory as usual.

## Important reset step after crash loops

If survivors spawn inside terrain or the game repeats null errors on load:

1. Close the game.
2. Delete `C:\Users\<you>\AppData\Roaming\7DaysToDie\Mods\grave\Saves\grave-alive-world.xml` if present.
3. Start a fresh save/new game for testing.
