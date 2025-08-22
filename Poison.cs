using System.Collections.Generic;

namespace GameServer.Game.MsgServer.AttackHandler
{
    public class Poison
    {
        public unsafe static void Execute(Client.GameClient user, InteractQuery Attack, ServerSockets.Packet stream, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
        {
            Database.MagicType.Magic DBSpell = DBSpells[0];

            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                , 0, Attack.X, Attack.Y, (ushort)Role.Flags.SpellID.Poison
                , 0, 0);

            Role.IMapObj target;
            if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Monster))
            {
                MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
                if (Role.Core.GetDistance(user.Player.X, user.Player.Y, attacked.X, attacked.Y) < DBSpell.Range)
                {
                    if (CheckAttack.CanAttackMonster.Verified(user, attacked, DBSpell))
                    {
                        MsgSpellAnimation.SpellObj AnimationObj;
                        Calculate.Physical.OnMonster(user.Player, attacked, DBSpell, out AnimationObj);
                        ReceiveAttack.Monster.Execute(stream, AnimationObj, user, attacked);
                        AnimationObj.Damage = Calculate.Base.CalculateSoul(AnimationObj.Damage, 0);
                        MsgSpell.Targets.Enqueue(AnimationObj);
                        if (attacked.Alive)
                        {
                            attacked.PoisonLevel = 1;
                            attacked.AddFlag(MsgUpdate.Flags.Poisoned, 15, true, 3);
                        }
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

                        MsgSpellAnimation.SpellObj AnimationObj;
                        Calculate.Physical.OnPlayer(user.Player, attacked, DBSpell, out AnimationObj);
                        if (AnimationObj.Damage <= 0)
                        {
                            AnimationObj.Damage = 1;
                        }
                        ReceiveAttack.Player.Execute(AnimationObj, user, attacked);
                        AnimationObj.Damage = Calculate.Base.CalculateSoul(AnimationObj.Damage, 0);
                        MsgSpell.Targets.Enqueue(AnimationObj);
                        if (attacked.Alive)
                        {
                            attacked.PoisonLevel = 0;
                            attacked.AddFlag(MsgUpdate.Flags.Poisoned, 15, true, 3);
                        }
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
                        MsgSpellAnimation.SpellObj AnimationObj;
                        Calculate.Physical.OnNpcs(user.Player, attacked, DBSpell, out AnimationObj);
                        ReceiveAttack.Npc.Execute(stream, AnimationObj, user, attacked);
                        AnimationObj.Damage = Calculate.Base.CalculateSoul(AnimationObj.Damage, 0);
                        MsgSpell.Targets.Enqueue(AnimationObj);
                    }
                }

            }
            MsgSpell.SetStream(stream);
            MsgSpell.Send(user);

        }
    }
}
