using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client
{

    public class Main : BaseScript
    {
        static uint allies = 0;
        static uint enemy = 1;
        static List<Ped> peds = new List<Ped>();
        static List<Ped> enemyPeds = new List<Ped>();

        public Main()
        {
            Tick += CheckAim;
            API.RegisterCommand("bodyguard", new Action(spawnBodyguard), false);
            API.RegisterCommand("enemy", new Action(spawnEnemy), false);
            API.RegisterCommand("mypeds", new Action(showMyPeds), false);
            API.RegisterCommand("clearPeds", new Action(clearPeds), false);
            API.RegisterCommand("clearEnemyPeds", new Action(clearEnemyPeds), false);
            API.AddRelationshipGroup("enemy", ref enemy);
        }

        private async Task CheckAim()
        {
            await Delay(10);

            if (API.IsControlPressed(0, 74) && !API.IsPedInAnyVehicle(API.GetPlayerPed(-1), false))
            {
                for (int i = 0; i < peds.Count; i++)
                {
                    peds[i].Task.ClearAll();
                }
                Screen.ShowNotification("~r~Koruman ateş etmeyi durdurdu!");
            }

            int entity = 9999;
            if (API.GetEntityPlayerIsFreeAimingAt(API.PlayerId(), ref entity))
            {
                if (entity != 9999)
                {
                    if (API.DoesEntityExist(entity) && API.IsEntityAPed(entity))
                    {
                        if (API.IsPedInAnyVehicle(API.GetPlayerPed(-1), false))
                        {
                            for (int i = 0; i < peds.Count; i++)
                            {
                                if (peds[i].Handle != entity && API.IsControlPressed(0, 73))
                                    peds[i].Task.VehicleShootAtPed((Ped)Ped.FromHandle(entity));
                            }
                        }
                        else
                        {
                            for (int i = 0; i < peds.Count; i++)
                            {
                                if (peds[i].Handle != entity && API.IsControlPressed(0, 73))
                                    peds[i].Task.ShootAt((Ped)Ped.FromHandle(entity), 5000, FiringPattern.FullAuto);
                            }
                        }
                    }
                }
            }
        }

        private async static void spawnBodyguard()
        {
            API.RequestModel((uint)PedHash.FibSec01);
            while (!API.HasModelLoaded((uint)PedHash.FibSec01))
            {
                Debug.Write("Waiting for model to load");
                await BaseScript.Delay(100);
            }
            Entity plyr = Entity.FromHandle(API.GetPlayerPed(-1));
            API.AddRelationshipGroup("allies" + plyr.Handle, ref allies);
            Ped bodyguard = await World.CreatePed(PedHash.FibSec01, plyr.Position + (plyr.ForwardVector * 2));
            API.SetPedRelationshipGroupHash(plyr.Handle, allies);
            API.SetPedArmour(bodyguard.Handle, 200);
            API.SetPedMaxHealth(bodyguard.Handle, 200);
            API.SetPedAsGroupMember(bodyguard.Handle, API.GetPedGroupIndex(plyr.Handle));
            API.SetPedRelationshipGroupHash(bodyguard.Handle, allies);
            API.SetPedCombatAbility(bodyguard.Handle, 2);
            API.GiveWeaponToPed(bodyguard.Handle, (uint)WeaponHash.MicroSMG, 9999, false, true);
            API.GiveWeaponToPed(bodyguard.Handle, (uint)WeaponHash.SpecialCarbineMk2, 9999, false, true);
            API.GiveWeaponComponentToPed(bodyguard.Handle, (uint)WeaponHash.SpecialCarbineMk2, 0xDE1FA12C);
            API.GiveWeaponComponentToPed(bodyguard.Handle, (uint)WeaponHash.SpecialCarbineMk2, 0x503DEA90);
            API.GiveWeaponComponentToPed(bodyguard.Handle, (uint)WeaponHash.SpecialCarbineMk2, 0x7BC4CDDC);
            API.GiveWeaponComponentToPed(bodyguard.Handle, (uint)WeaponHash.SpecialCarbineMk2, 0xC66B6542);
            API.GiveWeaponComponentToPed(bodyguard.Handle, (uint)WeaponHash.SpecialCarbineMk2, 0x9D65907A);
            API.GiveWeaponComponentToPed(bodyguard.Handle, (uint)WeaponHash.SpecialCarbineMk2, 0x2E7957A);
            API.GiveWeaponComponentToPed(bodyguard.Handle, (uint)WeaponHash.SpecialCarbineMk2, 0xD40BB53B);
            API.GiveWeaponComponentToPed(bodyguard.Handle, (uint)WeaponHash.SpecialCarbineMk2, 0xF97F783B);
            int blipid = API.AddBlipForEntity(bodyguard.Handle);
            API.SetBlipSprite(blipid, 175);
            API.SetBlipColour(blipid, (int)BlipColor.MichaelBlue);

            API.BeginTextCommandSetBlipName("STRING");
            API.AddTextComponentString("Bodyguard");
            API.EndTextCommandSetBlipName(blipid);
            peds.Add(bodyguard);
            bodyguard.Task.LookAt(plyr);
            bodyguard.CanRagdoll = false;
            bodyguard.CanWearHelmet = true;
            bodyguard.CanBeShotInVehicle = true;

            Screen.ShowNotification("~r~Korumanı birine ateş ettirmek istiyorsan kişiye nişan alıp ~g~\"X\" ~r~tuşuna basınız!");
            Screen.ShowNotification("~r~Korumanın ateş etmesini durdurmak için ~g~\"H\" ~r~tuşuna basınız!");
        }

        private async static void spawnEnemy()
        {
            Ped player = Game.Player.Character;
            API.RequestModel((uint)PedHash.Agent);
            while (!API.HasModelLoaded((uint)PedHash.Agent))
            {
                Debug.Write("Waiting for model to load");
                await BaseScript.Delay(100);
            }
            Ped enemyPed = await World.CreatePed(PedHash.Agent, player.Position + (player.ForwardVector * 2));
            API.SetPedRelationshipGroupHash(enemyPed.Handle, enemy);
            API.SetPedArmour(enemyPed.Handle, 100);
            API.SetPedCombatAbility(enemyPed.Handle, 2);
            API.GiveWeaponToPed(enemyPed.Handle, (uint)WeaponHash.CarbineRifleMk2, 500, false, true);
            enemyPeds.Add(enemyPed);
        }

        private async static void showMyPeds()
        {
            for (int i = 0; i < peds.Count; i++)
            {
                Debug.Write((i + 1) + " => Health: " + peds[i].Health + " Armor: " + peds[i].Armor + "\n");
                if (peds[i].Health > 0 && peds[i].Health < 200)
                {
                    peds[i].Health = 200;
                    Debug.Write((i + 1) + " => healthed!" + "\n");
                }

                if (peds[i].Armor > 0 && peds[i].Armor < 200)
                {
                    peds[i].Armor = 200;
                    Debug.Write((i + 1) + " => armored!" + "\n");
                }
                API.AddAmmoToPed(peds[i].Handle, (uint)WeaponHash.SpecialCarbineMk2, 9999);
                API.AddAmmoToPed(peds[i].Handle, (uint)WeaponHash.MicroSMG, 9999);
            }
        }

        private async static void clearPeds()
        {
            for (int i = 0; i < peds.Count; i++)
            {
                peds[i].Delete();
            }
            peds.Clear();
        } 
        
        private async static void clearEnemyPeds()
        {
            for (int i = 0; i < enemyPeds.Count; i++)
            {
                enemyPeds[i].Delete();
            }
            enemyPeds.Clear();
        }
    }
}