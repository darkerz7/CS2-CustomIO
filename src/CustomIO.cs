﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2_CustomIO
{
	[MinimumApiVersion(322)]
	public class CustomIO : BasePlugin
	{
		static MemoryFunctionVoid<CEntityIdentity, CUtlSymbolLarge, CEntityInstance, CEntityInstance, CVariant, int> CEntityIdentity_AcceptInputFunc = new(GameData.GetSignature("CEntityIdentity_AcceptInput"));
		static MemoryFunctionVoid<CEntityIdentity, string> CEntityIdentity_SetEntityNameFunc = new(GameData.GetSignature("CEntityIdentity_SetEntityName"));
		static Action<CEntityIdentity, string> SetTargetName = CEntityIdentity_SetEntityNameFunc.Invoke;
		//static readonly Vector vec3_origin = new(0, 0, 0);
		//static Vector[] g_vecPlayerOriginalVelocity = new Vector[65]; //Vector Leak Fix
		public override string ModuleName => "Custom IO";
		public override string ModuleDescription => "Fixes missing keyvalues from CSS/CS:GO";
		public override string ModuleAuthor => "DarkerZ [RUS]";
		public override string ModuleVersion => "1.DZ.14";
		public override void Load(bool hotReload)
		{
			/*for (int i = 0; i < 65; i++)
			{
				g_vecPlayerOriginalVelocity[i] = new(0, 0, 0);
			}*/
			CEntityIdentity_AcceptInputFunc.Hook(OnInput, HookMode.Pre);
		}

		public override void Unload(bool hotReload)
		{
			CEntityIdentity_AcceptInputFunc.Unhook(OnInput, HookMode.Pre);
		}

		private HookResult OnInput(DynamicHook hook)
		{
			var cEntity = hook.GetParam<CEntityIdentity>(0);
			var sInput = hook.GetParam<CUtlSymbolLarge>(1).String;
			if (string.IsNullOrEmpty(sInput)) return HookResult.Continue;
			var cValue = hook.GetParam<CVariant>(4);
			var sValue = cValue.FieldType == fieldtype_t.FIELD_CSTRING ? NativeAPI.GetStringFromSymbolLarge(cValue.Handle) : "";

			if (sInput.StartsWith("keyvalue", StringComparison.OrdinalIgnoreCase))
			{
				if (!string.IsNullOrEmpty(sValue))
				{
					string[] keyvalue = sValue.Split([' ']);
					if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[0]))
					{
						switch (keyvalue[0].ToLower())
						{
							case "targetname": KV_Targetname(cEntity, keyvalue); break;
							case "origin": KV_Origin(new CBaseEntity(cEntity.EntityInstance.Handle), keyvalue); break;
							case "angles": KV_Angles(new CBaseEntity(cEntity.EntityInstance.Handle), keyvalue); break;
							case "max_health": KV_MaxHealth(new CBaseEntity(cEntity.EntityInstance.Handle), keyvalue); break;
							case "health": KV_Health(new CBaseEntity(cEntity.EntityInstance.Handle), keyvalue); break;
							case "movetype": KV_Movetype(new CBaseEntity(cEntity.EntityInstance.Handle), keyvalue); break;
							case "entitytemplate": KV_EntityTemplate(cEntity.EntityInstance, keyvalue); break;
							case "basevelocity": KV_Basevelocity(new CBaseEntity(cEntity.EntityInstance.Handle), keyvalue); break;
							case "absvelocity": KV_Absvelocity(new CBaseEntity(cEntity.EntityInstance.Handle), keyvalue); break;
							case "target": KV_Target(new CBaseEntity(cEntity.EntityInstance.Handle), keyvalue); break;
							case "filtername": KV_Filtername(new CBaseEntity(cEntity.EntityInstance.Handle), keyvalue); break;
							case "force": KV_Force(cEntity.EntityInstance, keyvalue); break;
							case "gravity": KV_Gravity(new CBaseEntity(cEntity.EntityInstance.Handle), keyvalue); break;
							case "timescale": KV_Timescale(new CBaseEntity(cEntity.EntityInstance.Handle), keyvalue); break;
							case "friction": KV_Friction(new CBaseEntity(cEntity.EntityInstance.Handle), keyvalue); break;
							case "speed": KV_Speed(cEntity.EntityInstance, keyvalue); break;
							case "runspeed": KV_Runspeed(cEntity.EntityInstance, keyvalue); break;
							case "damage": KV_Damage(new CBaseEntity(cEntity.EntityInstance.Handle), keyvalue); break;
							case "damagetype": KV_DamageType(new CBaseEntity(cEntity.EntityInstance.Handle), keyvalue); break;
							case "damageradius": KV_DamageRadius(new CBaseEntity(cEntity.EntityInstance.Handle), keyvalue); break;
							case "case": KV_Case(new CBaseEntity(cEntity.EntityInstance.Handle), keyvalue); break;

						}
					}
				}
			} else if (string.Equals(sInput.ToLower(), "addscore"))
			{
				var player = EntityIsPlayer(hook.GetParam<CEntityInstance>(2));
				if (player != null && Int32.TryParse(sValue, out int iscore))
				{
					player.Score += iscore;
					#if DEBUG
					PrintToConsole($"Player: {player.PlayerName}({player.SteamID}) AddScore: {iscore}");
					#endif
				}
			} else if (string.Equals(sInput.ToLower(), "setmessage") && string.Equals(cEntity.DesignerName, "env_hudhint"))
			{
				if(sValue != null) new CEnvHudHint(cEntity.EntityInstance.Handle).Message = sValue;
				#if DEBUG
				PrintToConsole($"env_hudhint({cEntity.Name}) SetMessage:{sValue}");
				#endif
			}
			else if (string.Equals(sInput.ToLower(), "setmodel"))
			{
				if (!string.IsNullOrEmpty(sValue))
				{
					var player = EntityIsPlayer(hook.GetParam<CEntityInstance>(2));
					if (player != null && player.PlayerPawn.Value != null && player.PlayerPawn.Value.IsValid)
					{
						player.PlayerPawn.Value.SetModel(sValue);

						//Fix moonwalking players after playermodel change
						/*g_vecPlayerOriginalVelocity[player.Slot].X = player.PlayerPawn.Value.AbsVelocity.X;
						g_vecPlayerOriginalVelocity[player.Slot].Y = player.PlayerPawn.Value.AbsVelocity.Y;
						g_vecPlayerOriginalVelocity[player.Slot].Z = player.PlayerPawn.Value.AbsVelocity.Z;
						player.PlayerPawn.Value.MoveType = MoveType_t.MOVETYPE_OBSOLETE;
						player.PlayerPawn.Value.Teleport(null, null, vec3_origin);
						_ = new CounterStrikeSharp.API.Modules.Timers.Timer(0.01f, () =>
						{
							if(player != null && player.PlayerPawn.Value != null && player.PlayerPawn.Value.IsValid && player.PawnIsAlive)
							{
								player.PlayerPawn.Value.MoveType = MoveType_t.MOVETYPE_WALK;
								player.PlayerPawn.Value.Teleport(null, null, g_vecPlayerOriginalVelocity[player.Slot]);
							}
						});*/
						#if DEBUG
						PrintToConsole($"Player: {player.PlayerName}({player.SteamID}) SetModel: {sValue}");
						#endif
					}
				}
			}

			return HookResult.Continue;
		}

		static void KV_Targetname(CEntityIdentity cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]))
			{
				#if DEBUG
				PrintToConsole($"DesignerName: {cEntity.DesignerName} OldTargetname: {cEntity.Name} NewTargetname: {keyvalue[1]}");
				#endif
				SetTargetName(cEntity, keyvalue[1]);
			}
		}

		static void KV_Origin(CBaseEntity cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 4 && !string.IsNullOrEmpty(keyvalue[1]) && !string.IsNullOrEmpty(keyvalue[2]) && !string.IsNullOrEmpty(keyvalue[3]))
			{
				if (float.TryParse(keyvalue[1], out float x) && float.TryParse(keyvalue[2], out float y) && float.TryParse(keyvalue[3], out float z))
				{
					x = Math.Clamp(x, -16384.0f, 16384.0f);
					y = Math.Clamp(y, -16384.0f, 16384.0f);
					z = Math.Clamp(z, -16384.0f, 16384.0f);
					Vector vecOrigin = new(x, y, z);
					cEntity.Teleport(vecOrigin);
					#if DEBUG
					PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} NewOrigin:{x} {y} {z}");
					#endif
				}
			}
		}

		static void KV_Angles(CBaseEntity cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 4 && !string.IsNullOrEmpty(keyvalue[1]) && !string.IsNullOrEmpty(keyvalue[2]) && !string.IsNullOrEmpty(keyvalue[3]))
			{
				if (float.TryParse(keyvalue[1], out float x) && float.TryParse(keyvalue[2], out float y) && float.TryParse(keyvalue[3], out float z))
				{
					x = Math.Clamp(x, -360.0f, 360.0f);
					y = Math.Clamp(y, -360.0f, 360.0f);
					z = Math.Clamp(z, -360.0f, 360.0f);
					QAngle vecAngle = new(x, y, z);
					cEntity.Teleport(null, vecAngle, null);
					#if DEBUG
					PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} NewAngle:{x} {y} {z}");
					#endif
				}
			}
		}

		static void KV_MaxHealth(CBaseEntity cEntity, string[] keyvalue)
		{
			int iMaxHealth = 100;
			if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]) && Int32.TryParse(keyvalue[1], out iMaxHealth))
			{
				cEntity.MaxHealth = iMaxHealth;
				#if DEBUG
				PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} MaxHealth:{iMaxHealth}");
				#endif
			}
		}

		static void KV_Health(CBaseEntity cEntity, string[] keyvalue)
		{
			int iHealth = 100;
			if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]) && Int32.TryParse(keyvalue[1], out iHealth))
			{
				cEntity.Health = iHealth;
				#if DEBUG
				PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} Health:{iHealth}");
				#endif
			}
		}

		/* public enum MoveType_t : byte
		 * {
		 *		MOVETYPE_NONE = 0,			MOVETYPE_OBSOLETE = 1,		MOVETYPE_WALK = 2,		MOVETYPE_FLY = 3,
		 *		MOVETYPE_FLYGRAVITY = 4,	MOVETYPE_VPHYSICS = 5,		MOVETYPE_PUSH = 6,		MOVETYPE_NOCLIP = 7,
		 *		MOVETYPE_OBSERVER = 8,		MOVETYPE_LADDER = 9,		MOVETYPE_CUSTOM = 10,	MOVETYPE_LAST = 11,
		 *		MOVETYPE_INVALID = 11,		MOVETYPE_MAX_BITS = 5
		 *	}*/
		static void KV_Movetype(CBaseEntity cEntity, string[] keyvalue)
		{
			var player = EntityIsPlayer(cEntity);
			if (player != null && player.PlayerPawn.Value != null && keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]))
			{
				if (byte.TryParse(keyvalue[1], out byte iMovetype))
				{
					iMovetype = Math.Clamp(iMovetype, (byte)MoveType_t.MOVETYPE_NONE, (byte)MoveType_t.MOVETYPE_LAST);
					player.PlayerPawn.Value.MoveType = (MoveType_t)iMovetype;
					Schema.SetSchemaValue(player.PlayerPawn.Value.Handle, "CBaseEntity", "m_nActualMoveType", (MoveType_t)iMovetype);
					Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_MoveType");
					#if DEBUG
					PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} MoveType: {(MoveType_t)iMovetype}");
					#endif
				}
			}
		}

		static void KV_EntityTemplate(CEntityInstance cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]) && string.Equals(cEntity.DesignerName, "env_entity_maker"))
			{
				CEnvEntityMaker maker = new(cEntity.Handle)
				{
					Template = keyvalue[1]
				};
				#if DEBUG
				PrintToConsole($"ENV_Entity_Maker: ID-{maker.Index} Name-{maker.Entity?.Name} EntityTemplate: {keyvalue[1]}");
				#endif
			}
		}

		static void KV_Basevelocity(CBaseEntity cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 4 && !string.IsNullOrEmpty(keyvalue[1]) && !string.IsNullOrEmpty(keyvalue[2]) && !string.IsNullOrEmpty(keyvalue[3]))
			{
				if (float.TryParse(keyvalue[1], out float x) && float.TryParse(keyvalue[2], out float y) && float.TryParse(keyvalue[3], out float z))
				{
					x = Math.Clamp(x, -4096.0f, 4096.0f);
					y = Math.Clamp(y, -4096.0f, 4096.0f);
					z = Math.Clamp(z, -4096.0f, 4096.0f);
					cEntity.BaseVelocity.X = x;
					cEntity.BaseVelocity.Y = y;
					cEntity.BaseVelocity.Z = z;
					#if DEBUG
					PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} BaseVelocity:{x} {y} {z}");
					#endif
				}
			}
		}

		static void KV_Absvelocity(CBaseEntity cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 4 && !string.IsNullOrEmpty(keyvalue[1]) && !string.IsNullOrEmpty(keyvalue[2]) && !string.IsNullOrEmpty(keyvalue[3]))
			{
				if (float.TryParse(keyvalue[1], out float x) && float.TryParse(keyvalue[2], out float y) && float.TryParse(keyvalue[3], out float z))
				{
					x = Math.Clamp(x, -4096.0f, 4096.0f);
					y = Math.Clamp(y, -4096.0f, 4096.0f);
					z = Math.Clamp(z, -4096.0f, 4096.0f);
					Vector vecAbsVelocity = new(x, y, z);
					cEntity.Teleport(null, null, vecAbsVelocity);
					#if DEBUG
					PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} AbsVelocity:{x} {y} {z}");
					#endif
				}
			}
		}

		static void KV_Target(CBaseEntity cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]) && FindEntityByName(keyvalue[1]) != null)
			{
				cEntity.Target = keyvalue[1];
				#if DEBUG
				PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} Target: {keyvalue[1]}");
				#endif
			}
		}

		static void KV_Filtername(CBaseEntity cEntity, string[] keyvalue)
		{
			if (cEntity.DesignerName.StartsWith("trigger_") && keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]) && FindEntityByName(keyvalue[1]) != null)
			{
				new CBaseTrigger(cEntity.Handle).FilterName = keyvalue[1];
				#if DEBUG
				PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} FilterName: {keyvalue[1]}");
				#endif
			}
		}

		static void KV_Force(CEntityInstance cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]) && string.Equals(cEntity.DesignerName, "phys_thruster"))
			{
				if (float.TryParse(keyvalue[1], out float fForce))
				{
					CPhysThruster cThruster = new(cEntity.Handle)
					{
						Force = fForce
					};
					#if DEBUG
					PrintToConsole($"phys_thruster: ID-{cThruster.Index} Name-{cThruster.Entity?.Name} Force: {fForce}");
					#endif
				}
			}
		}

		static void KV_Gravity(CBaseEntity cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]))
			{
				if (float.TryParse(keyvalue[1], out float fGravity))
				{
					cEntity.GravityScale = fGravity;
					#if DEBUG
					PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} Gravity: {fGravity}");
					#endif
				}
			}
		}

		static void KV_Timescale(CBaseEntity cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]))
			{
				if (float.TryParse(keyvalue[1], out float fTimeScale))
				{
					cEntity.TimeScale = fTimeScale;
					#if DEBUG
					PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} TimeScale: {fTimeScale}");
					#endif
				}
			}
		}

		static void KV_Friction(CBaseEntity cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]))
			{
				if (float.TryParse(keyvalue[1], out float fFriction))
				{
					cEntity.Friction = fFriction;
					#if DEBUG
					PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} Friction: {fFriction}");
					#endif
				}
			}
		}

		static void KV_Speed(CEntityInstance cEntity, string[] keyvalue)
		{
			var player = EntityIsPlayer(cEntity);
			if (player != null && player.PlayerPawn.Value != null && player.PlayerPawn.Value.MovementServices != null && keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]))
			{
				float fSpeed = 0.001f;
				if (float.TryParse(keyvalue[1], out fSpeed))
				{
					if(fSpeed <= 0.0f) fSpeed = 0.001f;
					player.PlayerPawn.Value.MovementServices.Maxspeed = 260.0f * fSpeed;
					player.PlayerPawn.Value.VelocityModifier = fSpeed;
					#if DEBUG
					PrintToConsole($"Player: {player.PlayerName}({player.SteamID}) Speed: {fSpeed}");
					#endif
				}
			}
		}

		static void KV_Runspeed(CEntityInstance cEntity, string[] keyvalue)
		{
			var player = EntityIsPlayer(cEntity);
			if (player != null && player.PlayerPawn.Value != null && keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]))
			{
				float fRunSpeed = 0.001f;
				if (float.TryParse(keyvalue[1], out fRunSpeed))
				{
					if (fRunSpeed <= 0.0f) fRunSpeed = 0.001f;
					player.PlayerPawn.Value.VelocityModifier = fRunSpeed;
					#if DEBUG
					PrintToConsole($"Player: {player.PlayerName}({player.SteamID}) RunSpeed: {fRunSpeed}");
					#endif
				}
			}
		}

		static void KV_Damage(CBaseEntity cEntity, string[] keyvalue)
		{
			if (string.Equals(cEntity.DesignerName, "point_hurt"))
			{
				if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]) && Int32.TryParse(keyvalue[1], out int iDamage))
				{
					new CPointHurt(cEntity.Handle).Damage = iDamage;
					#if DEBUG
					PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} Damage:{iDamage}");
					#endif
				}
			}
		}

		static void KV_DamageType(CBaseEntity cEntity, string[] keyvalue)
		{
			if (string.Equals(cEntity.DesignerName, "point_hurt"))
			{
				if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]) && Int32.TryParse(keyvalue[1], out int iBitsDamageType))
				{
					new CPointHurt(cEntity.Handle).BitsDamageType = (DamageTypes_t)iBitsDamageType;
					#if DEBUG
					PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} BitsDamageType:{iBitsDamageType}");
					#endif
				}
			}
		}

		static void KV_DamageRadius(CBaseEntity cEntity, string[] keyvalue)
		{
			if (string.Equals(cEntity.DesignerName, "point_hurt"))
			{
				if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]) && Int32.TryParse(keyvalue[1], out int iDamageRadius))
				{
					new CPointHurt(cEntity.Handle).Radius = iDamageRadius;
					#if DEBUG
					PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} DamageRadius:{iDamageRadius}");
					#endif
				}
			}
		}

		static void KV_Case(CBaseEntity cEntity, string[] keyvalue)
		{
			if (string.Equals(cEntity.DesignerName, "logic_case"))
			{
				if (keyvalue.Length >= 3 && !string.IsNullOrEmpty(keyvalue[1]) && !string.IsNullOrEmpty(keyvalue[2]) && Int32.TryParse(keyvalue[1], out int iCase)) // keyvalue[1]-iCase; keyvalue[2]-pValue
				{
					string sArgs = keyvalue[2];
					for (int i = 3; i < keyvalue.Length; i++)
					{
						if(!string.IsNullOrEmpty(keyvalue[i]))
							sArgs += " " + keyvalue[i];
					}
					if (iCase >= 1 && iCase <= 32)
					{
						new CLogicCase(cEntity.Handle).Case[iCase - 1] = sArgs;
						#if DEBUG
						PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} Case:{iCase} Value:{keyvalue[2]}");
						#endif
					}
				}
			}
		}

		static CEntityInstance? FindEntityByName(string sName)
		{
			foreach (CEntityInstance? entbuf in Utilities.GetAllEntities())
			{
				if (entbuf != null && entbuf.IsValid && entbuf.Entity != null && !string.IsNullOrEmpty(entbuf.Entity.Name) && string.Equals(entbuf.Entity.Name, sName)) return entbuf;
			}
			return null;
		}
		static CCSPlayerController? EntityIsPlayer(CEntityInstance? entity)
		{
			if (entity != null && entity.IsValid && string.Equals(entity.DesignerName, "player"))
			{
				var pawn = new CCSPlayerPawn(entity.Handle);
				if (pawn.Controller.Value != null && pawn.Controller.Value.IsValid)
				{
					var player = new CCSPlayerController(pawn.Controller.Value.Handle);
					if (player != null && player.IsValid) return player;
				}
			}
			return null;
		}
		#if DEBUG
		public static void PrintToConsole(string sMessage, int iColor = 12)
		{
			Console.ForegroundColor = (ConsoleColor)8;
			Console.Write("[");
			Console.ForegroundColor = (ConsoleColor)6;
			Console.Write("CustomIO:Debug");
			Console.ForegroundColor = (ConsoleColor)8;
			Console.Write("] ");
			Console.ForegroundColor = (ConsoleColor)iColor;
			Console.WriteLine(sMessage);
			Console.ForegroundColor = (ConsoleColor)1;
			/* Colors:
				* 0 - No color		1 - White		2 - Red-Orange		3 - Orange
				* 4 - Yellow		5 - Dark Green	6 - Green			7 - Light Green
				* 8 - Cyan			9 - Sky			10 - Light Blue		11 - Blue
				* 12 - Violet		13 - Pink		14 - Light Red		15 - Red */
		}
		#endif
	}
}
