using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Helpers;
using static PokemonGo.RocketAPI.GeneratedCode.InventoryResponse.Types;
using static PokemonGo.RocketAPI.GeneratedCode.InventoryResponse.Types.PokemonProto.Types;
using System.IO;

namespace PokemonGo.RocketAPI.Console
{
    class Program
    {
        public static string title = "PokeBot 1.2 | Current Module: ";
        static void Main(string[] args)
        {
            System.Console.Title = title + "Initialization";
            Task.Run(() => Execute());
            System.Console.ReadLine();
        }
        public static void ColoredConsoleWrite(ConsoleColor color, string text)
        {
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = color;
            System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}]" + text);
            System.Console.ForegroundColor = originalColor;
        }
        private static async Task EvolveAllGivenPokemons(Client client, IEnumerable<PokemonProto> pokemonToEvolve)
        {
            foreach (var pokemon in pokemonToEvolve)
            {
                /*
                enum Holoholo.Rpc.Types.EvolvePokemonOutProto.Result {
	                UNSET = 0;
	                SUCCESS = 1;
	                FAILED_POKEMON_MISSING = 2;
	                FAILED_INSUFFICIENT_RESOURCES = 3;
	                FAILED_POKEMON_CANNOT_EVOLVE = 4;
	                FAILED_POKEMON_IS_DEPLOYED = 5;
                }
                }*/

                var countOfEvolvedUnits = 0;
                var xpCount = 0;

                EvolvePokemonOutProto evolvePokemonOutProto;
                do
                {
                    evolvePokemonOutProto = await client.EvolvePokemon(pokemon.Id);
                        //todo: someone check whether this still works

                    if (evolvePokemonOutProto.Result == 1)
                    {
                        ColoredConsoleWrite(ConsoleColor.Cyan, 
                            $"[{DateTime.Now.ToString("HH:mm:ss")}] Evolved {pokemon.Id} successfully for {evolvePokemonOutProto.ExpAwarded}xp");

                        countOfEvolvedUnits++;
                        xpCount += evolvePokemonOutProto.ExpAwarded;
                    }
                    else
                    {
                        var result = evolvePokemonOutProto.Result;
                        /*
                        ColoredConsoleWrite(ConsoleColor.White, $"Failed to evolve {pokemon.PokemonId}. " +
                                                 $"EvolvePokemonOutProto.Result was {result}");

                        ColoredConsoleWrite(ConsoleColor.White, $"Due to above error, stopping evolving {pokemon.PokemonId}");
                        */
                    }
                } while (evolvePokemonOutProto.Result == 1);
                if (countOfEvolvedUnits > 0)
                    ColoredConsoleWrite(ConsoleColor.Cyan, 
                        $"[{DateTime.Now.ToString("HH:mm:ss")}] Evolved {countOfEvolvedUnits} pieces of {pokemon.Id} for {xpCount}xp");

                await Task.Delay(3000);
            }
        }
        static async void Execute()
        {
            
            if (File.Exists(@AppDomain.CurrentDomain.BaseDirectory + @"\gps.txt"))
            {
                string[] lines = File.ReadAllLines(@AppDomain.CurrentDomain.BaseDirectory + @"\gps.txt");
                var client = new Client(Convert.ToDouble(lines[0]), Convert.ToDouble(lines[1]));
                if (Settings.UsePTC)
                {
                    System.Console.Title = title + "PTCLogin";
                    await client.LoginPtc(Settings.PtcUsername, Settings.PtcPassword);
                }
                else
                {
                    //Check if refresh token file exists
                    if (File.Exists(@AppDomain.CurrentDomain.BaseDirectory + @"\token.txt"))
                    {
                        ColoredConsoleWrite(ConsoleColor.Green, "Using Refresh Token: " + File.ReadLines(@AppDomain.CurrentDomain.BaseDirectory + @"\token.txt").First());
                        client.GoogleLoginByRefreshToken(File.ReadLines(@AppDomain.CurrentDomain.BaseDirectory + @"\token.txt").First());
                    }
                    else
                    {
                        ColoredConsoleWrite(ConsoleColor.Red, "You will have to restart the application every 30 minutes because of google tokens. You won't need to do this every time tho. If you get errors delete the token.txt in the application folder.");
                        await client.LoginGoogle();
                    }

                }
                System.Console.Title = title + "API Initialization";
                var serverResponse = await client.GetServer();
                ColoredConsoleWrite(ConsoleColor.Yellow, "Server Fetched");
                var profile = await client.GetProfile();
                ColoredConsoleWrite(ConsoleColor.Yellow, "Profile Fetched");
                var settings = await client.GetSettings();
                ColoredConsoleWrite(ConsoleColor.Yellow, "Settings Fetched");
                var mapObjects = await client.GetMapObjects();
                ColoredConsoleWrite(ConsoleColor.Yellow, "Objects Fetched");
                var inventory = await client.GetInventory();
                ColoredConsoleWrite(ConsoleColor.Yellow, "Inventory Fetched");
                var pokemons = inventory.Payload[0].Bag.Items.Select(i => i.Item?.Pokemon).Where(p => p != null && p?.PokemonType != PokemonProto.Types.PokemonIds.PokemonUnset);
                System.Console.Title = title + "StartUp";
                ColoredConsoleWrite(ConsoleColor.Green, "Starting up! Credits: Original Bot by Neel, Mod by ShiftCode");
                await Task.Delay(5000);



                try
                {
                    System.Console.Title = title + "Farm";
                    ColoredConsoleWrite(ConsoleColor.Magenta, "[Loading Module 'Farm']");
                    ColoredConsoleWrite(ConsoleColor.Magenta, "['Farm' Loaded]");
                    await EvolveAllGivenPokemons(client, pokemons);
                    await Task.Delay(5000);
                    await ExecuteFarmingPokestopsAndPokemons(client);
                    System.Console.Title = title + "Stop?";
                    ColoredConsoleWrite(ConsoleColor.Red, "Unexpected stop? Restarting in 5 seconds.");
                    await Task.Delay(5000);
                    Execute();
                }
                catch (TaskCanceledException tce) { ColoredConsoleWrite(ConsoleColor.Red, "Task Canceled Exception - Restarting"); Execute(); }
                catch (UriFormatException ufe) { ColoredConsoleWrite(ConsoleColor.Red, "System URI Format Exception - Restarting"); Execute(); }
                catch (ArgumentOutOfRangeException aore) { ColoredConsoleWrite(ConsoleColor.Red, "ArgumentOutOfRangeException - Restarting"); Execute(); }
                catch (NullReferenceException nre) { ColoredConsoleWrite(ConsoleColor.Red, "Null Refference - Restarting"); Execute(); }
                //await ExecuteCatchAllNearbyPokemons(client);
            }
            else
            {
                var client = new Client(Settings.DefaultLatitude, Settings.DefaultLongitude);
                if (Settings.UsePTC)
                {
                    System.Console.Title = title + "PTCLogin";
                    await client.LoginPtc(Settings.PtcUsername, Settings.PtcPassword);
                }
                else
                {
                    //Check if refresh token file exists
                    if (File.Exists(@AppDomain.CurrentDomain.BaseDirectory + @"\token.txt"))
                    {
                        ColoredConsoleWrite(ConsoleColor.Green, "Using Refresh Token: " + File.ReadLines(@AppDomain.CurrentDomain.BaseDirectory + @"\token.txt").First());
                        client.GoogleLoginByRefreshToken(File.ReadLines(@AppDomain.CurrentDomain.BaseDirectory + @"\token.txt").First());
                    }
                    else
                    {
                        ColoredConsoleWrite(ConsoleColor.Red, "You will have to restart the application every 30 minutes because of google tokens. You won't need to do this every time tho. If you get errors delete the token.txt in the application folder.");
                        await client.LoginGoogle();
                    }

                }
                System.Console.Title = title + "API Initialization";
                var serverResponse = await client.GetServer();
                ColoredConsoleWrite(ConsoleColor.Yellow, "Server Fetched");
                var profile = await client.GetProfile();
                ColoredConsoleWrite(ConsoleColor.Yellow, "Profile Fetched");
                var settings = await client.GetSettings();
                ColoredConsoleWrite(ConsoleColor.Yellow, "Settings Fetched");
                var mapObjects = await client.GetMapObjects();
                ColoredConsoleWrite(ConsoleColor.Yellow, "Objects Fetched");
                var inventory = await client.GetInventory();
                ColoredConsoleWrite(ConsoleColor.Yellow, "Inventory Fetched");
                var pokemons = inventory.Payload[0].Bag.Items.Select(i => i.Item?.Pokemon).Where(p => p != null && p?.PokemonType != PokemonProto.Types.PokemonIds.PokemonUnset);
                System.Console.Title = title + "StartUp";
                ColoredConsoleWrite(ConsoleColor.Green, "Starting up! Credits: Original Bot by Neel, Mod by ShiftCode");
                await Task.Delay(5000);



                try
                {
                    System.Console.Title = title + "Farm";
                    ColoredConsoleWrite(ConsoleColor.Magenta, "[Loading Module 'Farm']");
                    ColoredConsoleWrite(ConsoleColor.Magenta, "['Farm' Loaded]");
                    //if (!File.Exists(@AppDomain.CurrentDomain.BaseDirectory + @"\donevolve.txt"))
                    //    await EvolveAllGivenPokemons(client, pokemons);
                    await Task.Delay(5000);
                    await ExecuteFarmingPokestopsAndPokemons(client);
                    System.Console.Title = title + "Stop?";
                    ColoredConsoleWrite(ConsoleColor.Red, "Unexpected stop? Restarting in 5 seconds.");
                    await Task.Delay(5000);
                    Execute();
                }
                catch (TaskCanceledException tce) { ColoredConsoleWrite(ConsoleColor.Red, "Task Canceled Exception - Restarting"); Execute(); }
                catch (UriFormatException ufe) { ColoredConsoleWrite(ConsoleColor.Red, "System URI Format Exception - Restarting"); Execute(); }
                catch (ArgumentOutOfRangeException aore) { ColoredConsoleWrite(ConsoleColor.Red, "ArgumentOutOfRangeException - Restarting"); Execute(); }
                catch (NullReferenceException nre) { ColoredConsoleWrite(ConsoleColor.Red, "Null Refference - Restarting"); Execute(); }
                //await ExecuteCatchAllNearbyPokemons(client);
            }



        }

        private static async Task ExecuteFarmingPokestopsAndPokemons(Client client)
        {
            var mapObjects = await client.GetMapObjects();
            var pokeStops = mapObjects.Payload[0].Profile.SelectMany(i => i.Fort).Where(i => i.FortType == (int)MiscEnums.FortType.CHECKPOINT && i.CooldownCompleteMs < DateTime.UtcNow.ToUnixTime());
            foreach (var pokeStop in pokeStops)
            {
                System.Console.Title = title + "Farming PokeStops";
                var update = await client.UpdatePlayerLocation(pokeStop.Latitude, pokeStop.Longitude);
                var fortInfo = await client.GetFort(pokeStop.FortId, pokeStop.Latitude, pokeStop.Longitude);
                var fortSearch = await client.SearchFort(pokeStop.FortId, pokeStop.Latitude, pokeStop.Longitude);
                var bag = fortSearch.Payload[0];

                ColoredConsoleWrite(ConsoleColor.DarkCyan, $"Farmed XP: {bag.XpAwarded}, Gems: { bag.GemsAwarded}, Eggs: {bag.EggPokemon} Items: {GetFriendlyItemsString(bag.Items)}");

                await ExecuteCatchAllNearbyPokemons(client);

                await Task.Delay(10000); // Delay for Pokestop Jumps
            }
        }

        private static async Task ExecuteCatchAllNearbyPokemons(Client client)
        {
            var mapObjects = await client.GetMapObjects();

            var pokemons = mapObjects.Payload[0].Profile.SelectMany(i => i.MapPokemon);

            foreach (var pokemon in pokemons)
            {
                System.Console.Title = title + "Catching Pokemon";
                var update = await client.UpdatePlayerLocation(pokemon.Latitude, pokemon.Longitude);
                var encounterPokemonRespone = await client.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnpointId);

                CatchPokemonResponse caughtPokemonResponse;
                do
                {
                    caughtPokemonResponse = await client.CatchPokemon(pokemon.EncounterId, pokemon.SpawnpointId, pokemon.Latitude, pokemon.Longitude);
                }
                while (caughtPokemonResponse.Payload[0].Status == 2);
                if(caughtPokemonResponse.Payload[0].Status == 1)
                    ColoredConsoleWrite(ConsoleColor.DarkGreen, (caughtPokemonResponse.Payload[0].Status == 1 ? $"We caught a {GetFriendlyPokemonName(pokemon.PokedexTypeId)}" : $"{GetFriendlyPokemonName(pokemon.PokedexTypeId)} got away.."));
                else
                    ColoredConsoleWrite(ConsoleColor.DarkRed, (caughtPokemonResponse.Payload[0].Status == 1 ? $"We caught a {GetFriendlyPokemonName(pokemon.PokedexTypeId)}" : $"{GetFriendlyPokemonName(pokemon.PokedexTypeId)} got away.."));
                await TransferAllButStrongestUnwantedPokemon(client);
                await Task.Delay(5000); // Delay for Catching Pokemon
            }
        }
        private static async Task TransferAllGivenPokemons(Client client, IEnumerable<PokemonProto> unwantedPokemons)
        {
            if (!File.Exists(@AppDomain.CurrentDomain.BaseDirectory + @"\donttransfer.txt"))
            {
                foreach (var pokemon in unwantedPokemons)
                {
                    System.Console.Title = title + "Transferring Pokemon";
                    var transferPokemonResponse = await client.TransferPokemon(pokemon.Id);

                    /*
                    ReleasePokemonOutProto.Status {
                        UNSET = 0;
                        SUCCESS = 1;
                        POKEMON_DEPLOYED = 2;
                        FAILED = 3;
                        ERROR_POKEMON_IS_EGG = 4;
                    }*/

                    if (transferPokemonResponse.Status == 1)
                    {
                        ColoredConsoleWrite(ConsoleColor.DarkGreen, $"transfered another {pokemon.PokemonType} to Professor.");
                    }
                    else
                    {
                        var status = transferPokemonResponse.Status;

                        ColoredConsoleWrite(ConsoleColor.DarkRed, $"Somehow failed to grind {pokemon.PokemonType}. " +
                                                 $"ReleasePokemonOutProto.Status was {status}");
                    }

                    await Task.Delay(3000);
                }
            }
                
        }

        

        private static async Task TransferAllButStrongestUnwantedPokemon(Client client)
        {

            
            // Below are the pokemon types that we are throwing away.
            var unwantedPokemonTypes = new[]
            {
                PokemonIds.V0016PokemonPidgey,
                PokemonIds.V0019PokemonRattata,
                PokemonIds.V0013PokemonWeedle,
                PokemonIds.V0041PokemonZubat,
                PokemonIds.V0010PokemonCaterpie,
                PokemonIds.V0017PokemonPidgeotto,
                PokemonIds.V0029PokemonNidoran,
                PokemonIds.V0046PokemonParas,
                PokemonIds.V0048PokemonVenonat,
                PokemonIds.V0054PokemonPsyduck,
                PokemonIds.V0060PokemonPoliwag,
                PokemonIds.V0079PokemonSlowpoke,
                PokemonIds.V0096PokemonDrowzee,
                PokemonIds.V0092PokemonGastly,
                PokemonIds.V0118PokemonGoldeen,
                PokemonIds.V0120PokemonStaryu,
                PokemonIds.V0129PokemonMagikarp,
                PokemonIds.V0133PokemonEevee,
                PokemonIds.V0147PokemonDratini,
                PokemonIds.V0100PokemonVoltorb,
                PokemonIds.V0081PokemonMagnemite,
                PokemonIds.V0021PokemonSpearow

            };

            var inventory = await client.GetInventory();
            var pokemons = inventory.Payload[0].Bag
                                .Items
                                .Select(i => i.Item?.Pokemon)
                                .Where(p => p != null && p?.PokemonType != PokemonIds.PokemonUnset)
                                .ToArray();

            foreach (var unwantedPokemonType in unwantedPokemonTypes)
            {
                System.Console.Title = title + "Transferring Pokemon";
                var pokemonOfDesiredType = pokemons.Where(p => p.PokemonType == unwantedPokemonType)
                                                   .OrderByDescending(p => p.Cp)
                                                   .ToList();

                var unwantedPokemon = pokemonOfDesiredType.Skip(1) // keep the strongest one for potential battle-evolving
                                                          .ToList();

               
                await TransferAllGivenPokemons(client, unwantedPokemon);
            }

            
        }

        private static string GetFriendlyPokemonName(MapObjectsResponse.Types.Payload.Types.PokemonIds id)
        {
            var name = Enum.GetName(typeof(PokemonProto.Types.PokemonIds), id);
            return name?.Substring(name.IndexOf("Pokemon") + 7);
        }

        private static string GetFriendlyItemsString(IEnumerable<FortSearchResponse.Types.Item> items)
        {
            var enumerable = items as IList<FortSearchResponse.Types.Item> ?? items.ToList();

            if (!enumerable.Any())
                return string.Empty;

            return
                enumerable.GroupBy(i => (MiscEnums.Item)i.Item_)
                          .Select(kvp => new { ItemName = kvp.Key.ToString(), Amount = kvp.Sum(x => x.ItemCount) })
                          .Select(y => $"{y.Amount} x {y.ItemName}")
                          .Aggregate((a, b) => $"{a}, {b}");
        }
    }
}