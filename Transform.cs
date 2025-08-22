using System.Collections.Generic;

namespace GameServer.Game.MsgServer.AttackHandler
{
    public class Transform
    {
        public unsafe static void Execute(Client.GameClient user, InteractQuery Attack, ServerSockets.Packet stream, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
        {
            if (user.Player.ContainFlag(MsgUpdate.Flags.Fly) || user.Player.ContainFlag(MsgUpdate.Flags.Ride) || user.Player.Map == 3820 || user.Player.Map == 501 ||  user.Player.Map == 1005 || user.Player.Map == 700 || user.Player.Map == 1036
               || user.Player.Map == 1080 || AttachStatus.BlockedFlyMap.Contains(user.Player.Map))
            {
                user.SendSysMesage("You cant use this skill right now !");
                return;
            }
            if (user.Player.ContainFlag(MsgUpdate.Flags.Fly) || user.Player.ContainFlag(MsgUpdate.Flags.Ride) || user.Player.Map == 1005 || user.Player.Map == 700 || user.Player.Map == 1036)
            {
                user.SendSysMesage("You cant use this skill right now !");
                return;
            }
            Database.MagicType.Magic DBSpell;
            MsgSpell ClientSpell;
            if (CheckAttack.CanUseSpell.Verified(Attack,user, DBSpells, out ClientSpell, out DBSpell))
            {
                uint Experience = 300;
                MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                    , 0, Attack.X, Attack.Y, ClientSpell.ID
                    , ClientSpell.Level, ClientSpell.UseSpellSoul);

                user.Player.RemoveFlag(MsgUpdate.Flags.XPList);

                Database.Tranformation.DBTranform Transform;
                if (Database.Tranformation.TransformInfo[(ushort)ClientSpell.ID].TryGetValue(DBSpell.Level, out Transform))
                {
                    user.Player.TransformInfo = new Role.ClientTransform(user.Player);
                    user.Player.TransformInfo.CreateTransform(stream,Transform.HitPoints, Transform.ID, (int)DBSpell.Duration);
                }
                MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, DBSpell.Duration, MsgAttackPacket.AttackEffect.None));
                MsgSpell.SetStream(stream); MsgSpell.Send(user);
               
                Updates.UpdateSpell.CheckUpdate(stream, user, Attack, Experience, DBSpells);
            }
        }
    }
}
