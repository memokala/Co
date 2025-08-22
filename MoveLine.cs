using System;
using System.Collections.Generic;

namespace GameServer.Game.MsgServer.AttackHandler
{
    public class MoveLine
    {
        public unsafe static void Execute(Client.GameClient user, InteractQuery Attack, ServerSockets.Packet stream, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
        {
            Database.MagicType.Magic DBSpell;
            MsgSpell ClientSpell;
            DateTime Now = DateTime.Now;

          
                #region Guildwar
                if (!CheckAttack.CheckFloors.CheckGuildWar(user, Attack.X, Attack.Y))
                {
                    return;
                }
                #endregion
                #region use stamina
                //if (user.Player.Stamina >= 20)
                //{
                //    user.Player.Stamina -= 20;
                //}
                #endregion
                #region Attack 
                if (Attack.X == user.Player.X || Attack.X == 0)
                    return;
                #endregion
                #region Algorithm
                Algoritms.InLineAlgorithm Line = new Algoritms.InLineAlgorithm(user.Player.X, Attack.X, user.Player.Y, Attack.Y, user.Map, (byte)12, 0);
                ushort X = (ushort)Line.lcoords[Line.lcoords.Count - 1].X;
                ushort Y = (ushort)Line.lcoords[Line.lcoords.Count - 1].Y;
                #endregion
                #region LCords
                if (!user.Map.ValidLocation(X, Y))
                {
                    if (Line.lcoords.Count >= 2)
                    {
                        X = (ushort)Line.lcoords[Line.lcoords.Count - 2].X;
                        Y = (ushort)Line.lcoords[Line.lcoords.Count - 2].Y;
                        if (!CheckAttack.CheckFloors.CheckGuildWar(user, X, Y))
                        {
                            return;
                        }
                    }
                    if (!user.Map.ValidLocation(X, Y))
                    {
                        if (Line.lcoords.Count >= 3)
                        {
                            X = (ushort)Line.lcoords[Line.lcoords.Count - 3].X;
                            Y = (ushort)Line.lcoords[Line.lcoords.Count - 3].Y;
                            if (!CheckAttack.CheckFloors.CheckGuildWar(user, X, Y))
                            {
                                return;
                            }
                        }
                    }
                }
                #endregion
                #region ValidLocation
                if (!user.Map.ValidLocation(X, Y))
                    return;
                #endregion
                #region CanUseSpell
                if (CheckAttack.CanUseSpell.Verified(Attack, user, DBSpells, out ClientSpell, out DBSpell))
                {
                    MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                                          , 0, Attack.X, Attack.Y, ClientSpell.ID
                                          , ClientSpell.Level, ClientSpell.UseSpellSoul);
                    MsgSpell.X = X;
                    MsgSpell.Y = Y;
                    user.Map.View.MoveTo<Role.IMapObj>(user.Player, MsgSpell.X, MsgSpell.Y);
                    user.Player.X = MsgSpell.X;
                    user.Player.Y = MsgSpell.Y;
                    uint Experience = 0;
                    foreach (Role.IMapObj target in user.Player.View.Roles(Role.MapObjectType.Monster))
                    {
                        if (Line.InLine(target.X, target.Y, (byte)DBSpell.MaxTargets))
                        {
                            MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
                            if (CheckAttack.CanAttackMonster.Verified(user, attacked, DBSpell))
                            {
                                MsgSpellAnimation.SpellObj AnimationObj;
                                Calculate.Physical.OnMonster(user.Player, attacked, DBSpell, out AnimationObj);
                                AnimationObj.Damage = Calculate.Base.CalculateSoul(AnimationObj.Damage, ClientSpell.UseSpellSoul);
                                Experience += ReceiveAttack.Monster.Execute(stream, AnimationObj, user, attacked);
                                MsgSpell.Targets.Enqueue(AnimationObj);

                                if (target.Alive)
                                {
                                    if (Role.Core.Rate(35))
                                    {
                                        attacked.BlackSpot = true;
                                        attacked.Stamp_BlackSpot = DateTime.Now.AddSeconds((int)DBSpell.Duration);

                                        user.Player.View.SendView(stream.BlackspotCreate(true, attacked.UID), true);
                                    }
                                }
                            }
                        }
                    }
                    foreach (Role.IMapObj targer in user.Player.View.Roles(Role.MapObjectType.Player))
                    {
                        if (Line.InLine(targer.X, targer.Y, (byte)DBSpell.MaxTargets))
                        {
                            var attacked = targer as Role.Player;
                            if (CheckAttack.CanAttackPlayer.Verified(user, attacked, DBSpell))
                            {
                                MsgSpellAnimation.SpellObj AnimationObj;
                                Calculate.Physical.OnPlayer(user.Player, attacked, DBSpell, out AnimationObj);
                                AnimationObj.Damage = Calculate.Base.CalculateSoul(AnimationObj.Damage, ClientSpell.UseSpellSoul);
                                AnimationObj.Damage = AnimationObj.Damage * 60 / 63;
                                ReceiveAttack.Player.Execute(AnimationObj, user, attacked);
                                MsgSpell.Targets.Enqueue(AnimationObj);
                                if (attacked.Alive)
                                {
                                    if (Role.Core.Rate(35))
                                    {
                                        attacked.BlackSpot = true;
                                        attacked.Stamp_BlackSpot = DateTime.Now.AddSeconds((int)DBSpell.Duration);
                                        user.Player.View.SendView(stream.BlackspotCreate(true, attacked.UID), true);
                                    }
                                }
                            }
                        }
                    }
                    foreach (Role.IMapObj targer in user.Player.View.Roles(Role.MapObjectType.SobNpc))
                    {
                        if (Line.InLine(targer.X, targer.Y, (byte)DBSpell.MaxTargets))
                        {
                            var attacked = targer as Role.SobNpc;
                            if (CheckAttack.CanAttackNpc.Verified(user, attacked, DBSpell))
                            {
                                MsgSpellAnimation.SpellObj AnimationObj;
                                Calculate.Physical.OnNpcs(user.Player, attacked, DBSpell, out AnimationObj);
                                AnimationObj.Damage = Calculate.Base.CalculateSoul(AnimationObj.Damage, ClientSpell.UseSpellSoul);
                                Experience += ReceiveAttack.Npc.Execute(stream, AnimationObj, user, attacked);
                                MsgSpell.Targets.Enqueue(AnimationObj);
                            }
                        }
                    }
                    Updates.IncreaseExperience.Up(stream, user, Experience);
                    Updates.UpdateSpell.CheckUpdate(stream, user, Attack, Experience, DBSpells);
                    MsgSpell.SetStream(stream);
                    MsgSpell.SendRole(user);
                    MsgSpell.Send(user);
                }
                #endregion
              

        }
    }
    
}
