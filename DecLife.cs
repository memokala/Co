﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgServer.AttackHandler
{
   public class DecLife
    {
       public unsafe static void Execute(Client.GameClient user, InteractQuery Attack, ServerSockets.Packet stream, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
       {
           Database.MagicType.Magic DBSpell;
           MsgSpell ClientSpell;
           if (CheckAttack.CanUseSpell.Verified(Attack,user, DBSpells, out ClientSpell, out DBSpell))
           {
               switch (ClientSpell.ID)
               {
                   case (ushort)Role.Flags.SpellID.GapingWounds:
                       {
                           MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                               , Attack.OpponentUID, Attack.X, Attack.Y, ClientSpell.ID
                               , ClientSpell.Level, ClientSpell.UseSpellSoul);
                           uint Experience = 0;
                           Role.IMapObj target;
                           if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Monster))
                           {
                               MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
                               if (Role.Core.GetDistance(user.Player.X, user.Player.Y, attacked.X, attacked.Y) < DBSpell.Range)
                               {
                                   if (CheckAttack.CanAttackMonster.Verified(user, attacked, DBSpell))
                                   {
                                       MsgSpellAnimation.SpellObj AnimationObj = new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None);

                                       if (attacked.Boss == 1)
                                           AnimationObj.Damage = 1;
                                       else
                                       {
                                           if (attacked.HitPoints > 1)
                                               AnimationObj.Damage = (uint)Calculate.Base.MulDiv((int)attacked.HitPoints, (int)(100 - DBSpell.Damage), 100);

                                           Experience += ReceiveAttack.Monster.Execute(stream, AnimationObj, user, attacked);
                                       }
                                       MsgSpell.Targets.Enqueue(AnimationObj);

                                   }
                               }
                           }
                           else if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                           {
                               var attacked = target as Role.Player;
                               if (Role.Core.GetDistance(user.Player.X, user.Player.Y, target.X, target.Y) < DBSpell.Range)
                               {

                                   if (CheckAttack.CanAttackPlayer.Verified(user, attacked, DBSpell))
                                   {
                                       MsgSpellAnimation.SpellObj AnimationObj = new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None);

                                       if (attacked.HitPoints > 1)
                                       {
                                           AnimationObj.Damage = (uint)Calculate.Base.MulDiv((int)attacked.HitPoints, (int)(100 - DBSpell.Damage ), 100);
                                           
                                       }

                                       ReceiveAttack.Player.Execute(AnimationObj, user, attacked);
                                       MsgSpell.Targets.Enqueue(AnimationObj);
                                   }
                               }

                           }
                           else if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.SobNpc))
                           {
                               var attacked = target as Role.SobNpc;
                               if (Role.Core.GetDistance(user.Player.X, user.Player.Y, target.X, target.Y) < DBSpell.Range)
                               {

                                   if (CheckAttack.CanAttackNpc.Verified(user, attacked, DBSpell))
                                   {
                                       MsgSpellAnimation.SpellObj AnimationObj = new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None);

                                       if (attacked.HitPoints > 1)
                                           AnimationObj.Damage = (uint)Calculate.Base.MulDiv((int)attacked.HitPoints, (int)(100 - DBSpell.Damage), 100);
                                       Experience += ReceiveAttack.Npc.Execute(stream,AnimationObj, user, attacked);
                                       MsgSpell.Targets.Enqueue(AnimationObj);
                                   }
                               }

                           }
                           Updates.IncreaseExperience.Up(stream,user, Experience);
                           Updates.UpdateSpell.CheckUpdate(stream,user,Attack, Experience, DBSpells);
                           MsgSpell.SetStream(stream);
                           MsgSpell.Send(user);

                           break;
                       }
                   case (ushort)Role.Flags.SpellID.CruelShade:
                       {
                           MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                               , Attack.OpponentUID, Attack.X, Attack.Y, ClientSpell.ID
                               , ClientSpell.Level, ClientSpell.UseSpellSoul);

                           uint Experience = 0;
                           Role.IMapObj target;
                           if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Monster))
                           {
                               MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
                               if (Role.Core.GetDistance(user.Player.X, user.Player.Y, attacked.X, attacked.Y) < DBSpell.Range)
                               {
                                   if (CheckAttack.CanAttackMonster.Verified(user, attacked, DBSpell))
                                   {
                                       MsgSpellAnimation.SpellObj AnimationObj = new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None);
                                       if (attacked.Boss == 1)
                                           AnimationObj.Damage = 1;
                                       else
                                       {
                                           if (attacked.HitPoints > 1)
                                               AnimationObj.Damage = (uint)Calculate.Base.MulDiv((int)attacked.HitPoints, (int)(100 - DBSpell.Damage), 100);
                                       }
                                       AnimationObj.Damage = Math.Min(AnimationObj.Damage, 3900);
                                       Experience += ReceiveAttack.Monster.Execute(stream, AnimationObj, user, attacked);
                                       MsgSpell.Targets.Enqueue(AnimationObj);
                                   }
                               }
                           }
                           else if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                           {
                               var attacked = target as Role.Player;
                               if (Role.Core.GetDistance(user.Player.X, user.Player.Y, target.X, target.Y) < DBSpell.Range)
                               {

                                   if (CheckAttack.CanAttackPlayer.Verified(user, attacked, DBSpell))
                                   {
                                       MsgSpellAnimation.SpellObj AnimationObj = new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None);

                                       if (attacked.HitPoints > 1)
                                       {
                                           AnimationObj.Damage = (uint)Calculate.Base.MulDiv((int)attacked.HitPoints, (int)(100 - DBSpell.Damage), 100);

                                       }
                                       AnimationObj.Damage = Math.Min(AnimationObj.Damage, 3900);
                                       ReceiveAttack.Player.Execute(AnimationObj, user, attacked);
                                       MsgSpell.Targets.Enqueue(AnimationObj);
                                   }
                               }

                           }
                           else if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.SobNpc))
                           {
                               var attacked = target as Role.SobNpc;
                               if (Role.Core.GetDistance(user.Player.X, user.Player.Y, target.X, target.Y) < DBSpell.Range)
                               {

                                   if (CheckAttack.CanAttackNpc.Verified(user, attacked, DBSpell))
                                   {
                                       MsgSpellAnimation.SpellObj AnimationObj = new MsgSpellAnimation.SpellObj(attacked.UID, 0, MsgAttackPacket.AttackEffect.None);
                                       if (attacked.HitPoints > 1)
                                           AnimationObj.Damage = (uint)Calculate.Base.MulDiv((int)attacked.HitPoints, (int)(100 - DBSpell.Damage), 100);
                                       AnimationObj.Damage = Math.Min(AnimationObj.Damage, 3900);
                                       Experience += ReceiveAttack.Npc.Execute(stream, AnimationObj, user, attacked);
                                       MsgSpell.Targets.Enqueue(AnimationObj);
                                   }
                               }

                           }
                           Updates.IncreaseExperience.Up(stream,user, Experience);
                           Updates.UpdateSpell.CheckUpdate(stream,user,Attack, Experience, DBSpells);
                           MsgSpell.SetStream(stream);
                           MsgSpell.Send(user);
                           break;
                       }
               }
           }
       }
    
    }
}
