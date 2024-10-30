using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2_CustomIO
{
	public class CustomIO : BasePlugin
	{
		public class CUtlSymbolLarge : NativeObject
		{
			public CUtlSymbolLarge(IntPtr pointer) : base(pointer) { }
			public string KeyValue => Utilities.ReadStringUtf8(Handle + 0);
		}
		public static MemoryFunctionVoid<CEntityIdentity, CUtlSymbolLarge, CEntityInstance, CEntityInstance, CVariant, int> CEntityIdentity_AcceptInputFunc = new(GameData.GetSignature("CEntityIdentity_AcceptInput"));
		public override string ModuleName => "Custom IO";
		public override string ModuleDescription => "Fixes missing keyvalues from CSS/CS:GO";
		public override string ModuleAuthor => "DarkerZ [RUS]";
		public override string ModuleVersion => "1.DZ.4";
		public override void Load(bool hotReload)
		{
			CEntityIdentity_AcceptInputFunc.Hook(OnInput, HookMode.Pre);
		}

		public override void Unload(bool hotReload)
		{
			CEntityIdentity_AcceptInputFunc.Unhook(OnInput, HookMode.Pre);
		}

		private HookResult OnInput(DynamicHook hook)
		{
			var cEntity = hook.GetParam<CEntityIdentity>(0);
			var cInput = hook.GetParam<CUtlSymbolLarge>(1);
			var cValue = new CUtlSymbolLarge(hook.GetParam<CVariant>(4).Handle);

			if (cInput.KeyValue.ToLower().StartsWith("keyvalue"))
			{
				if (!string.IsNullOrEmpty(cValue.KeyValue))
				{
					string[] keyvalue = cValue.KeyValue.Split([' ']);
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
							
						}
					}
				}
			} else if (cInput.KeyValue.ToLower().CompareTo("addscore") == 0)
			{
				var player = EntityIsPlayer(hook.GetParam<CEntityInstance>(2));
				int iscore = 0;
				if (player != null && Int32.TryParse(cValue.KeyValue, out iscore))
				{
					player.Score += iscore;
					#if DEBUG
					PrintToConsole($"Player: {player.PlayerName}({player.SteamID}) AddScore: {iscore}");
					#endif
				}
			} else if (cInput.KeyValue.ToLower().CompareTo("setmessage") == 0 && cEntity.DesignerName.CompareTo("env_hudhint") == 0)
			{
				new CEnvHudHint(cEntity.EntityInstance.Handle).Message = cInput.KeyValue;
				#if DEBUG
				PrintToConsole($"env_hudhint({cEntity.Name}) SetMessage:{cInput.KeyValue}");
				#endif
			} else if (cInput.KeyValue.ToLower().CompareTo("setmodel") == 0)
			{
				if (!string.IsNullOrEmpty(cValue.KeyValue))
				{
					var player = EntityIsPlayer(hook.GetParam<CEntityInstance>(2));
					if (player != null && player.PlayerPawn.Value != null && player.PlayerPawn.Value.IsValid)
					{
						player.PlayerPawn.Value.SetModel(cValue.KeyValue);
						#if DEBUG
						PrintToConsole($"Player: {player.PlayerName}({player.SteamID}) SetModel: {cValue.KeyValue}");
						#endif
					}
				}
			}

			return HookResult.Continue;
		}
		void KV_Targetname(CEntityIdentity cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]))
			{
				cEntity.Name = keyvalue[1];
				#if DEBUG
				PrintToConsole($"DesignerName: {cEntity.DesignerName} OldTargetname: {cEntity.Name} NewTargetname: {keyvalue[1]}");
				#endif
			}
		}
		void KV_Origin(CBaseEntity cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 4 && !string.IsNullOrEmpty(keyvalue[1]) && !string.IsNullOrEmpty(keyvalue[2]) && !string.IsNullOrEmpty(keyvalue[3]))
			{
				float x, y, z = 0.0f;
				if (float.TryParse(keyvalue[1], out x) && float.TryParse(keyvalue[2], out y) && float.TryParse(keyvalue[3], out z))
				{
					x = Math.Clamp(x, -16384.0f, 16384.0f);
					y = Math.Clamp(y, -16384.0f, 16384.0f);
					z = Math.Clamp(z, -16384.0f, 16384.0f);
					Vector vecOrigin = new Vector(x, y, z);
					cEntity.Teleport(vecOrigin);
					#if DEBUG
					PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} NewOrigin:{x} {y} {z}");
					#endif
				}
			}
		}
		void KV_Angles(CBaseEntity cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 4 && !string.IsNullOrEmpty(keyvalue[1]) && !string.IsNullOrEmpty(keyvalue[2]) && !string.IsNullOrEmpty(keyvalue[3]))
			{
				float x, y, z = 0.0f;
				if (float.TryParse(keyvalue[1], out x) && float.TryParse(keyvalue[2], out y) && float.TryParse(keyvalue[3], out z))
				{
					x = Math.Clamp(x, -360.0f, 360.0f);
					y = Math.Clamp(y, -360.0f, 360.0f);
					z = Math.Clamp(z, -360.0f, 360.0f);
					QAngle vecAngle = new QAngle(x, y, z);
					cEntity.Teleport(null, vecAngle, null);
					#if DEBUG
					PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} NewAngle:{x} {y} {z}");
					#endif
				}
			}
		}
		void KV_MaxHealth(CBaseEntity cEntity, string[] keyvalue)
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
		void KV_Health(CBaseEntity cEntity, string[] keyvalue)
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
		void KV_Movetype(CBaseEntity cEntity, string[] keyvalue)
		{
			var player = EntityIsPlayer(cEntity);
			if (player != null && player.PlayerPawn.Value != null && keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]))
			{
				byte iMovetype = 0;
				if (byte.TryParse(keyvalue[1], out iMovetype))
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
		void KV_EntityTemplate(CEntityInstance cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]) && cEntity.DesignerName.CompareTo("env_entity_maker") == 0)
			{
				CEnvEntityMaker maker = new CEnvEntityMaker(cEntity.Handle);
				maker.Template = keyvalue[1];
				#if DEBUG
				PrintToConsole($"ENV_Entity_Maker: ID-{maker.Index} Name-{maker.Entity?.Name} EntityTemplate: {keyvalue[1]}");
				#endif
			}
		}
		void KV_Basevelocity(CBaseEntity cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 4 && !string.IsNullOrEmpty(keyvalue[1]) && !string.IsNullOrEmpty(keyvalue[2]) && !string.IsNullOrEmpty(keyvalue[3]))
			{
				float x, y, z = 0.0f;
				if (float.TryParse(keyvalue[1], out x) && float.TryParse(keyvalue[2], out y) && float.TryParse(keyvalue[3], out z))
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
		void KV_Absvelocity(CBaseEntity cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 4 && !string.IsNullOrEmpty(keyvalue[1]) && !string.IsNullOrEmpty(keyvalue[2]) && !string.IsNullOrEmpty(keyvalue[3]))
			{
				float x, y, z = 0.0f;
				if (float.TryParse(keyvalue[1], out x) && float.TryParse(keyvalue[2], out y) && float.TryParse(keyvalue[3], out z))
				{
					x = Math.Clamp(x, -4096.0f, 4096.0f);
					y = Math.Clamp(y, -4096.0f, 4096.0f);
					z = Math.Clamp(z, -4096.0f, 4096.0f);
					Vector vecAbsVelocity = new Vector(x, y, z);
					cEntity.Teleport(null, null, vecAbsVelocity);
					#if DEBUG
					PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} AbsVelocity:{x} {y} {z}");
					#endif
				}
			}
		}
		void KV_Target(CBaseEntity cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]) && FindEntityByName(keyvalue[1]) != null)
			{
				cEntity.Target = keyvalue[1];
				#if DEBUG
				PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} Target: {keyvalue[1]}");
				#endif
			}
		}
		void KV_Filtername(CBaseEntity cEntity, string[] keyvalue)
		{
			if (cEntity.DesignerName.StartsWith("trigger_") && keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]) && FindEntityByName(keyvalue[1]) != null)
			{
				new CBaseTrigger(cEntity.Handle).FilterName = keyvalue[1];
				#if DEBUG
				PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} FilterName: {keyvalue[1]}");
				#endif
			}
		}
		void KV_Force(CEntityInstance cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]) && cEntity.DesignerName.CompareTo("phys_thruster") == 0)
			{
				float fForce = 0.0f;
				if (float.TryParse(keyvalue[1], out fForce))
				{
					CPhysThruster cThruster = new CPhysThruster(cEntity.Handle);
					cThruster.Force = fForce;
					#if DEBUG
					PrintToConsole($"phys_thruster: ID-{cThruster.Index} Name-{cThruster.Entity?.Name} Force: {fForce}");
					#endif
				}
			}
		}
		void KV_Gravity(CBaseEntity cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]))
			{
				float fGravity = 0.0f;
				if (float.TryParse(keyvalue[1], out fGravity))
				{
					cEntity.GravityScale = fGravity;
					#if DEBUG
					PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} Gravity: {fGravity}");
					#endif
				}
			}
		}
		void KV_Timescale(CBaseEntity cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]))
			{
				float fTimeScale = 0.0f;
				if (float.TryParse(keyvalue[1], out fTimeScale))
				{
					cEntity.TimeScale = fTimeScale;
					#if DEBUG
					PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} TimeScale: {fTimeScale}");
					#endif
				}
			}
		}
		void KV_Friction(CBaseEntity cEntity, string[] keyvalue)
		{
			if (keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]))
			{
				float fFriction = 0.0f;
				if (float.TryParse(keyvalue[1], out fFriction))
				{
					cEntity.Friction = fFriction;
					#if DEBUG
					PrintToConsole($"DesignerName: {cEntity.DesignerName} Name: {cEntity.Entity?.Name} Friction: {fFriction}");
					#endif
				}
			}
		}
		void KV_Speed(CEntityInstance cEntity, string[] keyvalue)
		{
			var player = EntityIsPlayer(cEntity);
			if (player != null && player.PlayerPawn.Value != null && player.PlayerPawn.Value.MovementServices != null && keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]))
			{
				float fSpeed = 0.0f;
				if (float.TryParse(keyvalue[1], out fSpeed))
				{
					player.PlayerPawn.Value.MovementServices.Maxspeed = 260.0f * fSpeed;
					player.PlayerPawn.Value.VelocityModifier = fSpeed;
					#if DEBUG
					PrintToConsole($"Player: {player.PlayerName}({player.SteamID}) Speed: {fSpeed}");
					#endif
				}
			}
		}
		void KV_Runspeed(CEntityInstance cEntity, string[] keyvalue)
		{
			var player = EntityIsPlayer(cEntity);
			if (player != null && player.PlayerPawn.Value != null && keyvalue.Length >= 2 && !string.IsNullOrEmpty(keyvalue[1]))
			{
				float fRunSpeed = 0.0f;
				if (float.TryParse(keyvalue[1], out fRunSpeed))
				{
					player.PlayerPawn.Value.VelocityModifier = fRunSpeed;
					#if DEBUG
					PrintToConsole($"Player: {player.PlayerName}({player.SteamID}) RunSpeed: {fRunSpeed}");
					#endif
				}
			}
		}

		CEntityInstance? FindEntityByName(string sName)
		{
			foreach (CEntityInstance? entbuf in Utilities.GetAllEntities())
			{
				if (entbuf != null && entbuf.IsValid && entbuf.Entity != null && !string.IsNullOrEmpty(entbuf.Entity.Name) && entbuf.Entity.Name.CompareTo(sName) == 0) return entbuf;
			}
			return null;
		}
		public CCSPlayerController? EntityIsPlayer(CEntityInstance? entity)
		{
			if (entity != null && entity.IsValid && entity.DesignerName.CompareTo("player") == 0)
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
