using System;
using XRL.Core;
using XRL.UI;
using System.Collections.Generic;
using XRL.World;

namespace XRL.World.Parts
{
	[Serializable]
	public class GelatinousCanteenDrinkable : IPart
	{
		public string FriendlyDescription;
		public string UnfriendlyDescription;

		public int UnfriendlyRepThreshold;
		private int LastRepOptimization = 9999;

		public override bool SameAs(IPart p)
		{
			return false;
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "CommandSmartUse");
			Object.RegisterPartEvent(this, "CanHaveSmartUseConversation");
			Object.RegisterPartEvent(this, "CanSmartUse");
			Object.RegisterPartEvent(this, "InvCommandPourObject");
			Object.RegisterPartEvent(this, "InvCommandDrinkObject");
			Object.RegisterPartEvent(this, "InvCommandCollectObject");
			Object.RegisterPartEvent(this, "InvCommandFillFrom");
			Object.RegisterPartEvent(this, "GetShortDescription");
			base.Register(Object);
		}
		public int GetFactionReputationFeeling(GameObject who)
		{
            return XRLCore.Core.Game.PlayerReputation.get("Oozes");
        }

		public bool GrantsWater(GameObject Who, bool DoPopup=true)
		{
			if (GetFactionReputationFeeling(Who) < UnfriendlyRepThreshold)
			{
				if (DoPopup && Who.IsPlayer())
				{
					Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + "&y" + ParentObject.GetVerb("recognize") + " your hostility towards slimes and refuses you access to its water.");
				}
				return false;
			}
			if ((ParentObject.GetPart("LiquidVolume") as LiquidVolume).Volume == 0)
			{
				if (DoPopup && Who.IsPlayer())
				{
					Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + "&y" + ParentObject.GetVerb("has") + " no more fresh water for you to drink.");
				}
				return false;
			}
			return true;
		}

		public bool Drink(GameObject Who)
		{
			Who.FireEvent(Event.New("InvCommandDrinkObject", "Owner", ParentObject));
			return true;
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "CanSmartUse")
			{
				if ((!E.GetGameObjectParameter("User").IsPlayer() || !ParentObject.IsPlayerLed()))
				{
					return false;
				}
			}
			else if (E.ID == "CanHaveSmartUseConversation")
			{
				return false;
			}
			else if (E.ID == "CommandSmartUse")
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("User");
				if (!gameObjectParameter.IsPlayer() || !ParentObject.IsPlayerLed())
				{
					Drink(gameObjectParameter);
				}
			}
			else if (
				(E.ID == "InvCommandPourObject" || E.ID == "InvCommandDrinkObject" || E.ID == "InvCommandCollectObject") 
				&& !GrantsWater(E.GetGameObjectParameter("Owner"))
			)
			{
				return false;
			}
			else if (E.ID == "InvCommandDrinkObject" && !GrantsWater(E.GetGameObjectParameter("Owner")))
			{
				return false;
			}
			else if (E.ID == "InvCommandFillFrom")
			{
				Popup.Show(ParentObject.The + ParentObject.DisplayNameOnlyDirect + "&y's cap " + ParentObject.GetVerb("elude") + " your grasp.");
				return false;
			}
			else if (E.ID == "GetShortDescription")
			{
				int Rep = GetFactionReputationFeeling(IPart.ThePlayer);
				if (LastRepOptimization != Rep) 
				{
					Description Desc = ParentObject.GetPart<Description>();
					if (GrantsWater(IPart.ThePlayer, DoPopup: false))
					{
						Desc.Short = FriendlyDescription;
						E.SetParameter("ShortDescription", FriendlyDescription);
					} else
					{
						Desc.Short = UnfriendlyDescription;
						E.SetParameter("ShortDescription", UnfriendlyDescription);
					}
					LastRepOptimization = Rep;
				}
			}
			return base.FireEvent(E);
		}
	}
}
