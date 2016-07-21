# Simple Pokemon Go Bot. C#.
Features:
- [x] Farm Pokestops
- [x] Catch Pokemon
- [x] Transfer Pokemon
- [x] Refresh token support. (for google auth)
- [x] Login with PTC [NOT in releases]
- [x] Login with Google
- [x] Hatch Eggs
- [ ] Evolve Pokemon [not working ATM, help wanted]
- [x] Add awesome colorZ
- [ ] Incubate New Eggs
- [ ] More proto

# .txt's
- [x] token.txt | Contains: 1 Line with Refresh Token | Don't delete!
- [x] gps.txt | Contains: 2 Lines with lat/lng coords. Dot/comma seems to be localized, try which one works. | Defaults to a 6 pokestop place in america
- [x] donttransfer.txt | Contains: nothing | Create to disable transfers
- [ ] dontevolve.txt | Contains: nothing | Create to disable evolving [Not needed, disabled automatically cuz not working]

# Setup Guide
- Download and extract the newest release [here](https://github.com/shiftcodeYT/PokemonGoBot/releases/latest)
- If you want to set your GPS location, create a GPS.txt with the following Content (find out which one works by running the program and checking the first line):
Line 1(lat): 12.3456
Line 2(lng): 45.6789
OR:
Line 1(lat): 12,3456
Line 2(lng): 45,6789
- If you don't want the bot to evolve Pokemon, create a empty text file called "dontevolve.txt" [Not needed, disabled automatically cuz not working]
- If you don't want the bot to transfer Pokemon, create a empty text file called "donttransfer.txt"
- Run the bot :)
