using GameServer.Game.MsgServer.AttackHandler.Algoritms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameServer.Game.MsgServer.MsgAttackPacket;
using static GameServer.Role.Flags;

namespace GameServer.Game.MsgServer.AttackHandler
{
	public class Assassin
	{
		public unsafe static void Execute(Client.GameClient user, InteractQuery Attack, ServerSockets.Packet stream, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
		{
			Database.MagicType.Magic DBSpell;
			MsgSpell ClientSpell;
			if (CheckAttack.CanUseSpell.Verified(Attack, user, DBSpells, out ClientSpell, out DBSpell))
			{
				switch (ClientSpell.ID)
				{					
					case (ushort)Role.Flags.SpellID.BladeFlurry: //XP Skill
						{							
							MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
								  , 0, Attack.X, Attack.Y, ClientSpell.ID
								  , ClientSpell.Level, ClientSpell.UseSpellSoul);

							if (user.Player.RemoveFlag(MsgUpdate.Flags.XPList))
							{
								if (!user.Player.ContainFlag(MsgUpdate.Flags.BladeFlurry))
									user.Player.OpenXpSkill(MsgUpdate.Flags.BladeFlurry, 50);
								else
								{
									user.Player.RemoveFlag(MsgUpdate.Flags.BladeFlurry);
									user.Player.OpenXpSkill(MsgUpdate.Flags.BladeFlurry, 50);
								}
								return;
							}

							uint Experience = 0;
							foreach (Role.IMapObj target in user.Player.View.Roles(Role.MapObjectType.Monster))
							{
								MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
								if (Calculate.Base.GetDistance(Attack.X, Attack.Y, attacked.X, attacked.Y) <= 10)
								{
									if (CheckAttack.CanAttackMonster.Verified(user, attacked, DBSpell))
									{
										MsgSpellAnimation.SpellObj AnimationObj;
										Calculate.Physical.OnMonster(user.Player, attacked, DBSpell, out AnimationObj);
										AnimationObj.Damage = Calculate.Base.CalculateSoul(AnimationObj.Damage, ClientSpell.UseSpellSoul);
										Experience += ReceiveAttack.Monster.Execute(stream, AnimationObj, user, attacked);
										MsgSpell.Targets.Enqueue(AnimationObj);
									}
								}
							}
							foreach (Role.IMapObj targer in user.Player.View.Roles(Role.MapObjectType.Player))
							{
								var attacked = targer as Role.Player;
								if (Calculate.Base.GetDistance(Attack.X, Attack.Y, attacked.X, attacked.Y) <= 10)
								{
									if (CheckAttack.CanAttackPlayer.Verified(user, attacked, DBSpell))
									{
										MsgSpellAnimation.SpellObj AnimationObj;
										Calculate.Physical.OnPlayer(user.Player, attacked, DBSpell, out AnimationObj);
										AnimationObj.Damage = Calculate.Base.CalculateSoul(AnimationObj.Damage, ClientSpell.UseSpellSoul);
										ReceiveAttack.Player.Execute(AnimationObj, user, attacked);
										MsgSpell.Targets.Enqueue(AnimationObj);
									}
								}

							}
							foreach (Role.IMapObj targer in user.Player.View.Roles(Role.MapObjectType.SobNpc))
							{
								var attacked = targer as Role.SobNpc;
								if (Calculate.Base.GetDistance(Attack.X, Attack.Y, attacked.X, attacked.Y) <= 10)
								{
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
							MsgSpell.Send(user);

							break;
						}
					case (ushort)Role.Flags.SpellID.BlisteringWave:
						{
							MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
								, 0, Attack.X, Attack.Y, ClientSpell.ID
								, ClientSpell.Level, ClientSpell.UseSpellSoul);
							RangeMove moveIn = new RangeMove();
							List<RangeMove.Coords> ranger = moveIn.MoveCoords(user.Player.X, user.Player.Y, Attack.X, Attack.Y, 7);
							uint Experience = 0;
							foreach (Role.IMapObj target in user.Player.View.Roles(Role.MapObjectType.Monster))
							{
								if (MsgSpell.Targets.Count >= 30)
									break;
								MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
								if (!moveIn.InRange(attacked.X, attacked.Y, 5, ranger)) continue;
								{
									if (Role.Core.GetDistance(attacked.X, attacked.Y, user.Player.X, user.Player.Y) < 15)
									{
										if (CheckAttack.CanAttackMonster.Verified(user, attacked, DBSpell))
										{
											MsgSpellAnimation.SpellObj AnimationObj;
											Calculate.Range.OnMonster(user.Player, attacked, DBSpell, out AnimationObj);
											AnimationObj.Damage = Calculate.Base.CalculateSoul(AnimationObj.Damage, ClientSpell.UseSpellSoul);
											Experience += ReceiveAttack.Monster.Execute(stream, AnimationObj, user, attacked);
											MsgSpell.Targets.Enqueue(AnimationObj);
										}
									}
								}
							}
							foreach (Role.IMapObj targer in user.Player.View.Roles(Role.MapObjectType.Player))
							{
								var attacked = targer as Role.Player;
								if (!moveIn.InRange(attacked.X, attacked.Y, 5, ranger)) continue;
								{
									if (CheckAttack.CanAttackPlayer.Verified(user, attacked, DBSpell))
									{
										MsgSpellAnimation.SpellObj AnimationObj;
										Calculate.Range.OnPlayer(user.Player, attacked, DBSpell, out AnimationObj);
										AnimationObj.Damage = Calculate.Base.CalculateSoul(AnimationObj.Damage, ClientSpell.UseSpellSoul);
										ReceiveAttack.Player.Execute(AnimationObj, user, attacked);
										AnimationObj.Hit = 0;
										MsgSpell.Targets.Enqueue(AnimationObj);
									}
								}
							}
							foreach (Role.IMapObj targer in user.Player.View.Roles(Role.MapObjectType.SobNpc))
							{
								var attacked = targer as Role.SobNpc;
								if (!moveIn.InRange(attacked.X, attacked.Y, 5, ranger)) continue;
								{
									if (CheckAttack.CanAttackNpc.Verified(user, attacked, DBSpell))
									{
										MsgSpellAnimation.SpellObj AnimationObj;
										Calculate.Range.OnNpcs(user.Player, attacked, DBSpell, out AnimationObj);
										AnimationObj.Damage = Calculate.Base.CalculateSoul(AnimationObj.Damage, ClientSpell.UseSpellSoul);
										Experience += ReceiveAttack.Npc.Execute(stream, AnimationObj, user, attacked);
										MsgSpell.Targets.Enqueue(AnimationObj);
									}
								}
							}
							Updates.IncreaseExperience.Up(stream, user, Experience);
							Updates.UpdateSpell.CheckUpdate(stream, user, Attack, Experience, DBSpells);
							MsgSpell.SetStream(stream);
							MsgSpell.Send(user);
							break;
						}
					case (ushort)Role.Flags.SpellID.KineticSpark:
						{
							MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID, 0, Attack.X, Attack.Y, ClientSpell.ID, ClientSpell.Level, ClientSpell.UseSpellSoul);
							MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0, MsgAttackPacket.AttackEffect.None));

							if (user.Player.ContainFlag(MsgUpdate.Flags.KineticSpark))
								user.Player.RemoveFlag(MsgUpdate.Flags.KineticSpark);
							else
								user.Player.AddFlag(MsgUpdate.Flags.KineticSpark, Role.StatusFlagsBigVector32.PermanentFlag, true);

							MsgSpell.SetStream(stream);
							MsgSpell.Send(user);							
							break;
						}
					case (ushort)Role.Flags.SpellID.DaggerStorm:
						{					

							MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
								, user.Player.UID, Attack.X, Attack.Y, ClientSpell.ID
								, ClientSpell.Level, ClientSpell.UseSpellSoul);

							uint Experience = 0;
							MsgServer.MsgGameItem item = new MsgServer.MsgGameItem();
							item.Color = (Role.Flags.Color)2;
							MsgFloorItem.MsgItem DropItem = null;
							if (MsgSpell.SpellLevel == 1)
								item.ITEM_ID = 41;
							else if (MsgSpell.SpellLevel == 2)
								item.ITEM_ID = 42;
							else
								item.ITEM_ID = 1027;

							foreach (Role.IMapObj target in user.Player.View.Roles(Role.MapObjectType.Monster))
							{
								MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
								if (Calculate.Base.GetDistance(Attack.X, Attack.Y, attacked.X, attacked.Y) <= 3)
								{
									if (CheckAttack.CanAttackMonster.Verified(user, attacked, DBSpell))
									{
										MsgSpellAnimation.SpellObj AnimationObj;
										Calculate.Physical.OnMonster(user.Player, attacked, DBSpell, out AnimationObj);
										AnimationObj.Damage = Calculate.Base.CalculateSoul(AnimationObj.Damage, ClientSpell.UseSpellSoul);
										Experience += ReceiveAttack.Monster.Execute(stream, AnimationObj, user, attacked);
										MsgSpell.Targets.Enqueue(AnimationObj);
										DropItem = new MsgFloorItem.MsgItem(item, Attack.X, Attack.Y, MsgFloorItem.MsgItem.ItemType.Effect, 0, 0, user.Player.Map, 0, false, user.Map, 4);
									}
								}
							}
							foreach (Role.IMapObj targer in user.Player.View.Roles(Role.MapObjectType.Player))
							{
								var attacked = targer as Role.Player;
								if (Calculate.Base.GetDistance(Attack.X, Attack.Y, attacked.X, attacked.Y) <= 3)
								{
									if (CheckAttack.CanAttackPlayer.Verified(user, attacked, DBSpell))
									{
										MsgSpellAnimation.SpellObj AnimationObj;
										Calculate.Physical.OnPlayer(user.Player, attacked, DBSpell, out AnimationObj);
										AnimationObj.Damage = Calculate.Base.CalculateSoul(AnimationObj.Damage, ClientSpell.UseSpellSoul);
										ReceiveAttack.Player.Execute(AnimationObj, user, attacked);
										MsgSpell.Targets.Enqueue(AnimationObj);
										DropItem = new MsgFloorItem.MsgItem(item, Attack.X, Attack.Y, MsgFloorItem.MsgItem.ItemType.Effect, 0, 0, user.Player.Map, 0, false, user.Map, 4);
									}
								}

							}
							foreach (Role.IMapObj targer in user.Player.View.Roles(Role.MapObjectType.SobNpc))
							{
								var attacked = targer as Role.SobNpc;
								if (Calculate.Base.GetDistance(Attack.X, Attack.Y, attacked.X, attacked.Y) <= 3)
								{
									if (CheckAttack.CanAttackNpc.Verified(user, attacked, DBSpell))
									{
										MsgSpellAnimation.SpellObj AnimationObj;
										Calculate.Physical.OnNpcs(user.Player, attacked, DBSpell, out AnimationObj);
										AnimationObj.Damage = Calculate.Base.CalculateSoul(AnimationObj.Damage, ClientSpell.UseSpellSoul);
										Experience += ReceiveAttack.Npc.Execute(stream, AnimationObj, user, attacked);
										MsgSpell.Targets.Enqueue(AnimationObj);
										DropItem = new MsgFloorItem.MsgItem(item, Attack.X, Attack.Y, MsgFloorItem.MsgItem.ItemType.Effect, 0, 0, user.Player.Map, 0, false, user.Map, 4);
									}
								}
							}
							Updates.IncreaseExperience.Up(stream, user, Experience);
							Updates.UpdateSpell.CheckUpdate(stream, user, Attack, Experience, DBSpells);
							MsgSpell.SetStream(stream);
							MsgSpell.Send(user);

							if (DropItem != null && user.Map.EnqueueItem(DropItem))
								DropItem.SendAll(stream, MsgFloorItem.MsgDropID.Effect);
							break;
						}
				}
			}
		}
	}
}
