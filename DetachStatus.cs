using System.Collections.Generic;

namespace GameServer.Game.MsgServer.AttackHandler
{
    public class DetachStatus
    {
        public unsafe static void Execute(Client.GameClient user, InteractQuery Attack, ServerSockets.Packet stream, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
        {
            Database.MagicType.Magic DBSpell;
            MsgSpell ClientSpell;
            if (CheckAttack.CanUseSpell.Verified(Attack, user, DBSpells, out ClientSpell, out DBSpell))
            {
                switch (ClientSpell.ID)
                {
                    case (ushort)Role.Flags.SpellID.ArcherBane:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);


                            Role.IMapObj target;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                            {
                                Role.Player attacked = target as Role.Player;
                                if (attacked.ContainFlag(MsgUpdate.Flags.Fly))
                                {
                                    if (CheckAttack.CanAttackPlayer.Verified(user, attacked, DBSpell))
                                    {
                                        //if (Calculate.Base.Success(70))
                                        {
                                            attacked.RemoveFlag(MsgUpdate.Flags.Fly);
                                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 30, MsgAttackPacket.AttackEffect.None));
                                        }
                                    }
                                    //else
                                    //{
                                    //    var clientobj = new MsgSpellAnimation.SpellObj(attacked.UID, MsgSpell.SpellID, MsgAttackPacket.AttackEffect.None);
                                    //    clientobj.Hit = 0;
                                    //    MsgSpell.Targets.Enqueue(clientobj);
                                    //}
                                }
                            }
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, 250, DBSpells);

                            break;
                        }
                    #region revivir 100%
                    case (ushort)Role.Flags.SpellID.Revive:
                        {
                            if(user.IsWatching())
                            {
                                user.SendSysMesage("This spell not work on this map");
                                break;
                            }
                            #region Animaciones
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                           , 0, Attack.X, Attack.Y, ClientSpell.ID
                           , ClientSpell.Level, ClientSpell.UseSpellSoul);
                            #endregion
                            user.Player.RemoveFlag(MsgUpdate.Flags.XPList);
                            Role.IMapObj target;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                            {
                                Role.Player attacked = target as Role.Player;

                                if (!attacked.Alive)//alive!
                                {
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None));
                                    attacked.Revive(stream);
                                }
                                else
                                {
                                    user.SendSysMesage("You can`t revive an alive player.");
                                    break;
                                }
                            }


                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, DBSpell.Duration, DBSpells);

                            break;
                        }
                    #endregion
                    case (ushort)Role.Flags.SpellID.Pray:
                        {
                            if (user.IsWatching()/* || user.Player.Map == 1005*/)
                            {
                                user.SendSysMesage("This spell not work on this map..");
                                break;
                            }
                            if (user.Player.Name.Contains("[GM]"))
                                break;
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);



                            Role.IMapObj target;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                            {
                                Role.Player attacked = target as Role.Player;
                                if (!attacked.Alive)
                                {
                                    if(MsgTournaments.MsgSchedules.CaptureTheFlag.Proces == MsgTournaments.ProcesType.Alive)
                                    {//alive!
                                        if (user.Map.ID == Game.MsgTournaments.MsgCaptureTheFlag.MapID && user.Player.DynamicID == 0)
                                        {
                                            if (user.Player.MyGuild != null && user.Player.MyGuildMember != null)
                                                user.Player.MyGuildMember.CTF_Exploits += 2;
                                        }
                                    }
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None));
                                    attacked.Revive(stream);
                                }
                                else
                                {
                                    user.Player.Mana += DBSpell.UseMana;
                                    user.SendSysMesage("You can`t revive an alive player.");
                                    break;
                                }
                            }
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);
                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, DBSpell.Duration, DBSpells);
                            break;
                        }
                }
            }
        }
    }
}
