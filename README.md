# Grave

Grave is a lightweight XML-only modlet for **7 Days to Die**.

The first version makes early-game inventory management less tedious by raising
the wood stack size to `10000`. It uses the standard modlet layout, so it does
not overwrite any vanilla game files and can be removed cleanly.

## What it changes

- `resourceWood` stack size: vanilla value -> `10000`

## Installation

1. Open your 7 Days to Die install directory.
2. Create a `Mods` directory if it does not already exist.
3. Copy this repository folder into `Mods` as `grave`.
4. Confirm the final layout looks like this:

   ```text
   7 Days To Die/
   └── Mods/
       └── grave/
           ├── ModInfo.xml
           ├── README.md
           └── Config/
               └── items.xml
   ```

5. Start the game or dedicated server and check the log for the loaded `grave`
   modlet.

Because this modlet only edits XML and does not add custom assets or code, it is
intended to work server-side for standard dedicated-server setups.

## Compatibility

- Uses the Alpha 21 / 1.0+ `ModInfo.xml` v2 format.
- Confirmed XPath target: `resourceWood` in `Data/Config/items.xml`.

If you want to expand this mod, verify exact item names in the game's own
`Data/Config/items.xml` before adding more XPath patches. 7 Days to Die XML
names are case-sensitive.

## Customizing

Edit `Config/items.xml` and change this value:

```xml
<set xpath="/items/item[@name='resourceWood']/property[@name='Stacknumber']/@value">10000</set>
```

For example, replace `10000` with `5000` for a smaller stack size.
