using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgServer.AttackHandler
{
    public class Riding
    {
        public unsafe static void Execute(Client.GameClient user, InteractQuery Attack, ServerSockets.Packet stream, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
        {
            Database.MagicType.Magic DBSpell;
            MsgSpell ClientSpell;
            if (DateTime.Now < user.Player.KickedOffSteed.AddSeconds(15))
            {
                user.SendSysMesage($"You have to wait {(int)(user.Player.KickedOffSteed.AddSeconds(15) - DateTime.Now).TotalSeconds} seconds to ride again.");
                return;
            }
            if (CheckAttack.CanUseSpell.Verified(Attack, user, DBSpells, out ClientSpell, out DBSpell))
            {
                if (user.Player.ContainFlag(MsgUpdate.Flags.Fly) || user.Player.OnTransform)
                {
                    user.SendSysMesage("You can`t use this skill right now !");
                    return;
                }
                if (user.Player.Map == 1508 || user.Player.Map == 1860 || user.Player.Map == 1005 || user.Player.Map == 501 || user.Player.Map == 2071 || user.Player.Map == 5263 || user.Player.Map == 1767 || user.Player.Map == 1764 || user.Player.Map == 1858 || user.Player.Map == 8881 || user.Player.Map == 8880 || user.Player.Map == 1038 || user.Player.Map == 700 || MsgTournaments.MsgSchedules.CurrentTournament.InTournament(user))
                {
                    user.SendSysMesage("You can't use this skill on this map.");
                    return;
                }

                MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                                 , 0, Attack.X, Attack.Y, ClientSpell.ID
                                 , ClientSpell.Level, ClientSpell.UseSpellSoul);

                if (!user.Player.ContainFlag(MsgUpdate.Flags.Ride))
                {
                    user.Player.AddFlag(MsgUpdate.Flags.Ride, Role.StatusFlagsBigVector32.PermanentFlag, true, 1);

                    user.Vigor = user.Status.MaxVigor;

                    user.Send(stream.ServerInfoCreate(MsgServerInfo.Action.Vigor, user.Vigor));

                }
                else
                    user.Player.RemoveFlag(MsgUpdate.Flags.Ride);
                MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0, MsgAttackPacket.AttackEffect.None));
                MsgSpell.SetStream(stream);
                MsgSpell.Send(user);

            }
        }
    }
}
