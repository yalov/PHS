
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

       
        [GameParameters.CustomIntParameterUI("Panda Center Base Cost", minValue = 12500, maxValue = 100000, stepSize = 500,
              toolTip = "This is the base cost for each Kerbal when using the Panda Center Base Cost")]
        public int const_cost = 25000;


        [GameParameters.CustomParameterUI("Disable all modifiers and discounts")]
        public bool disableAllModifiers = false;

        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            disableAllModifiers = false;
            const_cost = 25000;
            KStockCost = false;
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
            HireSettings2.disabled = disableAllModifiers;
            HireSettings3.disabled = disableAllModifiers;
            return true;
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }
    }


    public class HireSettings2 : GameParameters.CustomParameterNode
    {

        public override string Title { get { return "Modifiers"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "TRP Hire"; } }
        public override string DisplaySection { get { return "TRP Hire"; } }
        public override int SectionOrder { get { return 2; } }
        public override bool HasPresets { get { return true; } }

        internal static bool disabled = false;

        [GameParameters.CustomParameterUI("Set values to default")]
        public bool DefaultSettings = false;

        [GameParameters.CustomFloatParameterUI("Gender Cost Modifier", minValue = 1f, maxValue = 2f, displayFormat = "N2",
            toolTip = "The privilege of selecting the sex costs kredits")]
        public double gender_coef = 1.25f;

        [GameParameters.CustomFloatParameterUI("Low Quality Cost Modifier", minValue = 0.25f, maxValue = 1f, displayFormat = "N2",
            toolTip = "Cost Modifier of Low Quality kerbal (Stupidity = 100, Courage = 0)")]
        public double low_quality = 0.5f;

        [GameParameters.CustomFloatParameterUI("High Quality Cost Modifier", minValue = 1f, maxValue = 4f, displayFormat = "N2",
            toolTip = "Cost Modifier of High Quality kerbal (Stupidity = 100, Courage = 0)")]
        public double high_quality = 2f;

        [GameParameters.CustomFloatParameterUI("Fearless Cost Modifier", minValue = 1.0f, maxValue = 4f, displayFormat = "N2",
            toolTip ="Fearless kerbals get a higher pay, based on this value.")]
        public double fearless_coef = 2f;

        [GameParameters.CustomFloatParameterUI("LevelUp Cost Modifier", minValue = 0.0f, maxValue = 3f, displayFormat = "N2",
            toolTip = "More skilled kerbals get a higher pay, based on this value.")]
        public double levelup_coef = 1f;




        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            DefaultSettings = false;

            low_quality = 0.5f;
            high_quality = 2f;
            fearless_coef = 2f;
            gender_coef = 1.25f;
            levelup_coef = 1f;
        }


        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (disabled)
                return false;

            return true;
        }


        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "DefaultSettings" && DefaultSettings)
                SetDifficultyPreset(parameters.preset);
            return true;
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }
    }


    public class HireSettings3 : GameParameters.CustomParameterNode
    {

        public override string Title { get { return "Discounts"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "TRP Hire"; } }
        public override string DisplaySection { get { return "TRP Hire"; } }
        public override int SectionOrder { get { return 3; } }
        public override bool HasPresets { get { return true; } }

        internal static bool disabled = false;

        [GameParameters.CustomParameterUI("Set values to default")]
        public bool DefaultSettings = false;

        [GameParameters.CustomFloatParameterUI("Bulk (5-9) Discount (%)", minValue = 0f, maxValue = 40f,
            toolTip = "Hiring multiple kerbals at the same time saves Kredits in the hiring process")]
        public double bulk_discount1 = 15f;

        [GameParameters.CustomFloatParameterUI("Bulk (10) Discount (%)", minValue = 0.0f, maxValue = 40f,
             toolTip = "Hiring multiple kerbals at the same time saves Kredits in the hiring process")]
        public double bulk_discount2 = 30f;

        [GameParameters.CustomFloatParameterUI("Black Munday Discount (%)", minValue = 0f, maxValue = 40f,
            toolTip = "Discount on day when eclipse occurs")]
        public double black_discount = 10f;

        [GameParameters.CustomFloatParameterUI("New Year Discount (%)", minValue = 0f, maxValue = 80f,
            toolTip = "Discount within 3 days of every year-end")]
        public double new_year_discount = 50f;

        [GameParameters.CustomFloatParameterUI("Maximum Discount Allowed (%)", minValue = 25f, maxValue = 90f,
            toolTip = "The final discount can include multiple discounts added together.  The maximum discount will never exceed this value")]
        public double max_discount = 90f;


        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            DefaultSettings = false;

            bulk_discount1 = 15f;
            bulk_discount2 = 30f;
            black_discount = 10f;
            new_year_discount = 50f;
            max_discount = 90f;
        }


        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (disabled)
                return false;

            return true;
        }


        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (bulk_discount2 < bulk_discount1)
                bulk_discount2 = bulk_discount1;
            if (member.Name == "DefaultSettings" && DefaultSettings)
                SetDifficultyPreset(parameters.preset);
            return true;
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }
    }

}
