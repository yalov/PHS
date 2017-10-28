
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;


namespace Hire
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class HireSettings : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "TRP Hire"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "TRP Hire"; } }
        public override string DisplaySection { get { return "TRP Hire"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return true; } }

        [GameParameters.CustomParameterUI("Use Stock Center cost for Kerbals",
            toolTip ="Using the Stock Center costs will result in varying base costs for each Kerbal, based on how many Kerbals are active")]
        public bool KStockCost = false;

        [GameParameters.CustomFloatParameterUI("Panda Center Base Cost", minValue = 12500f, maxValue = 100000f, stepCount = 500,
            toolTip ="This is the base cost for each Kerbal when using the Panda Center Base Cost")]                                   // stepCount don't work 
        public double const_cost = 25000f;

        [GameParameters.CustomParameterUI("Disable all modifiers and discounts")]
        public bool disableAllModifiers = false;


        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            disableAllModifiers = false;
            const_cost = 25000;

            switch (preset)
            {
                case GameParameters.Preset.Easy:
                    KStockCost = false;                 
                    break;
                case GameParameters.Preset.Normal:
                    KStockCost = false;
                    break;
                case GameParameters.Preset.Moderate:
                    KStockCost = false;
                    break;
                case GameParameters.Preset.Hard:
                    KStockCost = true;   // Hard
                    break;
            }
        }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (KStockCost && member.Name == "const_cost")
                return false;
            if (member.Name == "KStockCost" || member.Name == "const_cost")
                return true;

            return true;
        }


        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            return true;
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }
    }


    public class HireSettings2 : GameParameters.CustomParameterNode
    {

        public override string Title { get { return "Modifiers and Discounts"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "TRP Hire"; } }
        public override string DisplaySection { get { return "TRP Hire"; } }
        public override int SectionOrder { get { return 2; } }
        public override bool HasPresets { get { return true; } }

        [GameParameters.CustomParameterUI("Set values to default")]
        public bool DefaultSettings = false;

        [GameParameters.CustomFloatParameterUI("Fearless Cost Modifier (%)", minValue = 100f, maxValue = 300f, stepCount = 5)]
        public double fearless_coef = 200f;

        [GameParameters.CustomFloatParameterUI("Gender Cost Modifier (%)",   minValue = 100f, maxValue = 200f, stepCount = 5)]
        public double gender_coef = 125f;

        [GameParameters.CustomFloatParameterUI("Bulk (5-9) Discount (%)", minValue = 0f, maxValue = 40f)]
        public double bulk_discount1 = 15f;

        [GameParameters.CustomFloatParameterUI("Bulk (10) Discount (%)", minValue = 0.0f, maxValue = 40f)]
        public double bulk_discount2 = 30f;

        [GameParameters.CustomFloatParameterUI("Black Munday Discount (%)", minValue = 0f, maxValue = 40f,
            toolTip = "Discount on day when eclipse occurs")]
        public double black_discount = 10f;

        [GameParameters.CustomFloatParameterUI("New Year Discount (%)", minValue = 0f, maxValue = 80f,
            toolTip = "Discount within 3 days of every year-end")]
        public double new_year_discount = 50f;

        [GameParameters.CustomFloatParameterUI("Maximum Discount Allowed (%)", minValue = 25f, maxValue = 99f,
            toolTip = "The final discount can include multiple discounts added together.  The maximum discount will never exceed this value")]
        public double max_discount = 90f;   // default max_discount value need to be exactly max sum of default discount added together (30+10+50) 


        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            DefaultSettings = false;

            fearless_coef = 200f;
            gender_coef = 125f;
            bulk_discount1 = 15f;
            bulk_discount2 = 30f;
            black_discount = 10f;
            new_year_discount = 50f;
            max_discount = 90f;
        }


        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "DefaultSettings" && DefaultSettings)
                SetDifficultyPreset(parameters.preset);

            if (bulk_discount2 < bulk_discount1)
                bulk_discount2 = bulk_discount1;

            if (HighLogic.CurrentGame.Parameters.CustomParams<HireSettings>().disableAllModifiers 
                        && member.Name != "disableAllModifiers"
                        && member.Name != "KStockCost"
                        && member.Name != "const_cost")
                return false;

            return true;
        }


        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            return true;
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }
    }
}
