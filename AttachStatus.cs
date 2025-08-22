using System;
using System.Collections.Generic;

namespace GameServer.Game.MsgServer.AttackHandler
{
    public class AttachStatus
    {
        public static List<uint> BlockedFlyMap = new List<uint>() { 1090, 1080, 3820, 2022, 5263,1767, 2071 };

        public unsafe static void Execute(Client.GameClient user, InteractQuery Attack, ServerSockets.Packet stream, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
        {
            Database.MagicType.Magic DBSpell;
            MsgSpell ClientSpell;
            if (CheckAttack.CanUseSpell.Verified(Attack, user, DBSpells, out ClientSpell, out DBSpell))
            {
                switch (ClientSpell.ID)
                {
                    case (ushort)Role.Flags.SpellID.Intensify:
                        {
                            Attack.SpellID = ClientSpell.ID;
                            Attack.SpellLevel = ClientSpell.Level;
                            user.Player.View.SendView(stream.InteractionCreate(&Attack), true);
                            user.Player.IntensifyStamp = DateTime.Now;
                            user.Player.InUseIntensify = true;
                            user.Player.IntensifyDamage = (int)DBSpell.Damage;

                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, DBSpell.Duration, DBSpells);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.DefensiveStance:
                        {
                            if (user.Player.ContainFlag(MsgUpdate.Flags.Ride))
                                user.Player.RemoveFlag(MsgUpdate.Flags.Ride);

                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                                , 0, Attack.X, Attack.Y, ClientSpell.ID
                                , ClientSpell.Level, ClientSpell.UseSpellSoul);


                            if (!user.Player.RemoveFlag(MsgUpdate.Flags.DefensiveStance))
                            {
                                user.Player.AddFlag(MsgUpdate.Flags.DefensiveStance, (int)DBSpell.Duration, false);
                                user.Player.SendUpdate(stream, Game.MsgServer.MsgUpdate.Flags.DefensiveStance, (uint)DBSpell.Duration
                                  , (uint)DBSpell.Damage, ClientSpell.Level, Game.MsgServer.MsgUpdate.DataType.DefensiveStance, true);
                            }
                            else
                            {
                                user.Player.RemoveFlag(MsgUpdate.Flags.DefensiveStance);
                            }


                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0, MsgAttackPacket.AttackEffect.None));
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.PoisonStar:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);


                            Role.IMapObj target;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                            {

                                Role.Player attacked = target as Role.Player;
                                if (!CheckAttack.CanAttackPlayer.Verified(user, attacked, DBSpell))
                                    return;
                                var rate = (((user.Player.BattlePower - attacked.BattlePower) + 10) * 7);
                                if ((attacked.BattlePower - user.Player.BattlePower) >= 10)
                                    rate = 10; // Pity Success rate.
                                if (user.Player.BattlePower >= attacked.BattlePower) rate = 100;
                                if (Calculate.Base.Success(rate))
                                {
                                    attacked.AddSpellFlag(MsgUpdate.Flags.PoisonStar, (int)DBSpell.Duration, true);
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, DBSpell.Duration, MsgAttackPacket.AttackEffect.None));
                                }
                                else
                                {

                                    var clientobj = new MsgSpellAnimation.SpellObj(attacked.UID, MsgSpell.SpellID, MsgAttackPacket.AttackEffect.None);
                                    clientobj.Hit = 0;
                                    MsgSpell.Targets.Enqueue(clientobj);
                                }

                            }
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, 250, DBSpells);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Stigma:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            if (user.Player.UID == Attack.OpponentUID)
                            {
                                user.Player.AddSpellFlag(MsgUpdate.Flags.Stigma, (int)DBSpell.Duration, true);
                                MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0, MsgAttackPacket.AttackEffect.None));
                            }
                            else
                            {
                                Role.IMapObj target;
                                if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Monster))
                                {
                                    MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
                                    attacked.AddSpellFlag(MsgUpdate.Flags.Stigma, (int)DBSpell.Duration, true);
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None));
                                }
                                else if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                                {
                                    Role.Player attacked = target as Role.Player;
                                    attacked.AddSpellFlag(MsgUpdate.Flags.Stigma, (int)DBSpell.Duration, true);
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None));

                                }
                            }

                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, DBSpell.Duration, DBSpells);
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.MagicShield:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            if (user.Player.UID == Attack.OpponentUID)
                            {
                                if (!user.Player.ContainFlag(MsgUpdate.Flags.Shield))
                                {
                                    user.Player.AddSpellFlag(MsgUpdate.Flags.Shield, (int)DBSpell.Duration, true);
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0, MsgAttackPacket.AttackEffect.None));
                                }
                            }
                            else
                            {
                                Role.IMapObj target;
                                if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Monster))
                                {
                                    MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
                                    if (!attacked.ContainFlag(MsgUpdate.Flags.Shield))
                                    {
                                        attacked.AddSpellFlag(MsgUpdate.Flags.Shield, (int)DBSpell.Duration, true);
                                        MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None));
                                    }
                                }
                                else if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                                {
                                    Role.Player attacked = target as Role.Player;
                                    if (!attacked.ContainFlag(MsgUpdate.Flags.Shield))
                                    {
                                        attacked.AddSpellFlag(MsgUpdate.Flags.Shield, (int)DBSpell.Duration, true);
                                        MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None));
                                    }
                                }
                            }

                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, DBSpell.Duration, DBSpells);
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.SoulShackle:
                        {

                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            Role.IMapObj target;
                            bool pass = false;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                            {
                                if (target.Alive)
                                    return;
                                if (user.Player.Map == 1005 || user.Player.Map == 700)
                                    return;
                                Role.Player attacked = target as Role.Player;

                                if (!attacked.ContainFlag(MsgUpdate.Flags.SoulShackle) && DateTime.Now > attacked.RemovedShackle.AddSeconds(2))
                                {
                                    var rate = (((user.Player.BattlePower - attacked.BattlePower) + 10) * 7);
                                    if ((attacked.BattlePower - user.Player.BattlePower) >= 10)
                                    {
                                        user.SendSysMesage("Your BattlePower is lower than your opponent get a better BattlePower to be able to use this skill on him .", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                                        break;
                                    }
                                    if (user.Player.BattlePower >= attacked.BattlePower) rate = 100;
                                    if (rate == 100 || Role.Core.Rate(rate))
                                    {
                                        attacked.SendUpdate(stream, MsgUpdate.Flags.SoulShackle, DBSpell.Duration, 0, ClientSpell.Level, MsgUpdate.DataType.SoulShackle, false);

                                        attacked.AddSpellFlag(MsgUpdate.Flags.SoulShackle, (int)DBSpell.Duration, true);
                                        if (MsgTournaments.MsgSchedules.CaptureTheFlag.Proces == Game.MsgTournaments.ProcesType.Alive)
                                        {
                                            if (user.Player.Map == Game.MsgTournaments.MsgCaptureTheFlag.MapID)
                                            {
                                                user.Player.MyGuildMember.CTF_Exploits += 1;
                                            }
                                        }
                                        pass = true;
                                        MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None));
                                    }
                                    else
                                        user.SendSysMesage("[Miss] You failed to shackle him due to BP difference.", MsgMessage.ChatMode.TopLeftSystem, MsgMessage.MsgColor.red);

                                }
                            }

                            if (!pass)
                                return;
                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, DBSpell.Duration, DBSpells);
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.StarofAccuracy:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            if (user.Player.UID == Attack.OpponentUID)
                            {
                                user.Player.AddSpellFlag(MsgUpdate.Flags.StarOfAccuracy, (int)DBSpell.Duration, true);
                                MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0, MsgAttackPacket.AttackEffect.None));
                            }
                            else
                            {
                                Role.IMapObj target;
                                if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Monster))
                                {
                                    MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
                                    attacked.AddSpellFlag(MsgUpdate.Flags.StarOfAccuracy, (int)DBSpell.Duration, true);
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None));
                                }
                                else if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                                {
                                    Role.Player attacked = target as Role.Player;
                                    attacked.AddSpellFlag(MsgUpdate.Flags.StarOfAccuracy, (int)DBSpell.Duration, true);
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None));

                                }
                            }

                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, DBSpell.Duration, DBSpells);
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Invisibility:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            if (user.Player.UID == Attack.OpponentUID)
                            {
                                user.Player.AddSpellFlag(MsgUpdate.Flags.Invisibility, (int)DBSpell.Duration, true);
                                MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0, MsgAttackPacket.AttackEffect.None));
                            }
                            else
                            {
                                Role.IMapObj target;
                                if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Monster))
                                {
                                    MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
                                    attacked.AddSpellFlag(MsgUpdate.Flags.Invisibility, (int)DBSpell.Duration, true);
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None));
                                }
                                else if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                                {
                                    Role.Player attacked = target as Role.Player;
                                    attacked.AddSpellFlag(MsgUpdate.Flags.Invisibility, (int)DBSpell.Duration, true);
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None));

                                }
                            }

                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, DBSpell.Duration, DBSpells);
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);
                            break;
                        }
                    case (ushort)Role.Flags.SpellID.AzureShield:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            int Time = 15 * ClientSpell.Level + 30;
                            if (user.Player.UID == Attack.OpponentUID)
                            {
                                user.Player.AzureShieldLevel = (byte)ClientSpell.Level;
                                user.Player.AzureShieldDefence = (ushort)(3000 * ClientSpell.Level);
                                user.Player.SendUpdate(stream, MsgUpdate.Flags.AzureShield, (uint)Time, user.Player.AzureShieldDefence, user.Player.AzureShieldLevel, MsgUpdate.DataType.AzureShield, false);
                                user.Player.AddSpellFlag(MsgUpdate.Flags.AzureShield, (int)Time, true);
                                MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0, MsgAttackPacket.AttackEffect.None));
                            }
                            else
                            {
                                Role.IMapObj target;
                                if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                                {
                                    Role.Player attacked = target as Role.Player;
                                    attacked.AzureShieldLevel = (byte)ClientSpell.Level;
                                    attacked.AzureShieldDefence = (ushort)(3000 * ClientSpell.Level);
                                    user.Player.SendUpdate(stream, MsgUpdate.Flags.AzureShield, (uint)Time, user.Player.AzureShieldDefence, user.Player.AzureShieldLevel, MsgUpdate.DataType.AzureShield, false);
                                    attacked.AddSpellFlag(MsgUpdate.Flags.AzureShield, (int)Time, true);
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None));

                                }
                            }

                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, DBSpell.Duration, DBSpells);
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Shield:
                        {
                            if (user.Player.ContainFlag(MsgUpdate.Flags.XPList) == false)
                                break;
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            user.Player.RemoveFlag(MsgUpdate.Flags.XPList);
                            user.Player.AddFlag(MsgUpdate.Flags.Shield, (int)DBSpell.Duration, true);

                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, DBSpell.Duration, MsgAttackPacket.AttackEffect.None));
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Dodge:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            if (user.Player.UID == Attack.OpponentUID)
                            {
                                user.Player.AddSpellFlag(MsgUpdate.Flags.Dodge, (int)DBSpell.Duration, true);
                                MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0, MsgAttackPacket.AttackEffect.None));
                            }
                            else
                            {
                                Role.IMapObj target;
                                if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Monster))
                                {
                                    MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
                                    attacked.AddSpellFlag(MsgUpdate.Flags.Dodge, (int)DBSpell.Duration, true);
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None));
                                }
                                else if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                                {
                                    Role.Player attacked = target as Role.Player;
                                    attacked.AddSpellFlag(MsgUpdate.Flags.Dodge, (int)DBSpell.Duration, true);
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None));

                                }
                            }

                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, DBSpell.Duration, DBSpells);
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);
                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Accuracy:
                        {
                            if (user.Player.ContainFlag(MsgUpdate.Flags.XPList) == false)
                                break;
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            user.Player.RemoveFlag(MsgUpdate.Flags.XPList);
                            user.Player.AddFlag(MsgUpdate.Flags.StarOfAccuracy, (int)DBSpell.Duration, true);

                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0, MsgAttackPacket.AttackEffect.None));
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);


                            break;
                        }
                    case (ushort)Role.Flags.SpellID.XpFly:
                        {
                            if (user.Player.ContainFlag(MsgUpdate.Flags.XPList) == false)
                                break;
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);
                            if (user.Player.OnTransform || user.Player.ContainFlag(MsgUpdate.Flags.Ride) || BlockedFlyMap.Contains(user.Player.Map))
                            {
                                user.SendSysMesage("You can't use this skill right now!");
                                break;
                            }
                            if (user.Player.ContainFlag(MsgUpdate.Flags.MagicShield))
                            {
                                user.SendSysMesage("You can't fly while having magic shield on!");
                                break;
                            }
                            
                            if (user.Player.OnTransform || user.Player.ContainFlag(MsgUpdate.Flags.Ride))
                            {
                                user.SendSysMesage("You can't use this skill right now!");
                                break;
                            }
                            if (user.Player.ContainFlag(MsgUpdate.Flags.Fly))
                                user.Player.UpdateFlag(MsgUpdate.Flags.Fly, (int)DBSpell.Duration, true, 0);
                            else
                                user.Player.AddFlag(MsgUpdate.Flags.Fly, (int)DBSpell.Duration, true);

                            user.Player.RemoveFlag(MsgUpdate.Flags.XPList);


                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, DBSpell.Duration, MsgAttackPacket.AttackEffect.None));
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Fly:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);
                            if (user.Player.OnTransform || user.Player.ContainFlag(MsgUpdate.Flags.Ride) || BlockedFlyMap.Contains(user.Player.Map))
                            {
                                user.SendSysMesage("You can't use this skill right now!");
                                break;
                            }
                            
                            if (user.Player.OnTransform || user.Player.ContainFlag(MsgUpdate.Flags.Ride))
                            {
                                user.SendSysMesage("You can't use this skill right now!");
                                break;
                            }

                            if (user.Player.ContainFlag(MsgUpdate.Flags.Fly))
                                user.Player.UpdateFlag(MsgUpdate.Flags.Fly, (int)DBSpell.Duration, true, 0);
                            else
                                user.Player.AddFlag(MsgUpdate.Flags.Fly, (int)DBSpell.Duration, true);

                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, DBSpell.Duration, MsgAttackPacket.AttackEffect.None));
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Bless:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);


                            user.Player.AddFlag(MsgUpdate.Flags.CastPray, Role.StatusFlagsBigVector32.PermanentFlag, true);

                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0, MsgAttackPacket.AttackEffect.None));
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);


                            break;
                        }
                    case (ushort)Role.Flags.SpellID.FatalStrike:
                        {
                            if (user.Player.Map == 1038 || user.Player.Map == 3868)
                            {
                                user.SendSysMesage("You can't use this skill right now!");
                                break;

                            }
                            if (user.Player.ContainFlag(MsgUpdate.Flags.XPList) == false)
                                break;
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            user.Player.RemoveFlag(MsgUpdate.Flags.XPList);
                            user.Player.OpenXpSkill(MsgUpdate.Flags.FatalStrike, 60);

                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0, MsgAttackPacket.AttackEffect.None));
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);


                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Cyclone:
                        {
                            if (user.Player.ContainFlag(MsgUpdate.Flags.XPList) == false)
                                break;
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                             , 0, Attack.X, Attack.Y, ClientSpell.ID
                             , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            user.Player.RemoveFlag(MsgUpdate.Flags.XPList);
                            user.Player.OpenXpSkill(MsgUpdate.Flags.Cyclone, 20);

                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0, MsgAttackPacket.AttackEffect.None));
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);


                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Superman:
                        {
                            if (user.Player.ContainFlag(MsgUpdate.Flags.XPList) == false)
                                break;
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                          , 0, Attack.X, Attack.Y, ClientSpell.ID
                           , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            user.Player.RemoveFlag(MsgUpdate.Flags.XPList);
                            user.Player.OpenXpSkill(MsgUpdate.Flags.Superman, 20);

                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0, MsgAttackPacket.AttackEffect.None));
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            break;
                        }
					case (ushort)Role.Flags.SpellID.PathOfshadow:
						{
							var weps = user.Equipment;
							if ((weps.LeftWeapon != null && weps.LeftWeapon / 1000 != 613) || (weps.RightWeapon != null && weps.RightWeapon / 1000 != 613))
							{
								user.SendSysMesage("You need to wear only knifes!");
								return;
							}
							MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID, 0, Attack.X, Attack.Y, ClientSpell.ID, ClientSpell.Level, ClientSpell.UseSpellSoul);

							if (user.Player.ContainFlag(MsgUpdate.Flags.PathOfShadow))
							{
								if (user.Player.ContainFlag(MsgUpdate.Flags.BladeFlurry))
									user.Player.RemoveFlag(MsgUpdate.Flags.BladeFlurry);
								if (user.Player.ContainFlag(MsgUpdate.Flags.KineticSpark))
									user.Player.RemoveFlag(MsgUpdate.Flags.KineticSpark);

								user.Player.RemoveFlag(MsgUpdate.Flags.PathOfShadow);
							}
							else user.Player.AddFlag(MsgUpdate.Flags.PathOfShadow, Role.StatusFlagsBigVector32.PermanentFlag, false);
							MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0, MsgAttackPacket.AttackEffect.None));
							MsgSpell.SetStream(stream);
							MsgSpell.Send(user);
							break;
						}
				}
            }
        }
    }
}
