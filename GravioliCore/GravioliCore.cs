// 
//     
// 
//     Copyright (C) 2015 Sean McDougall
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GravioliCore
{
    public class GravioliCore : PartModule
    {
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Scale"), UI_FloatRange(minValue = 1f, maxValue = 100f, stepIncrement = 1f)]
        public float scaleFactor = 1;

        //[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Charge Factor"), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f)]
        public float chargeFactor = 0;

        //[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Heat Factor"), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f)]
        private float heatFactor = 0;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Polarity"), UI_Toggle(disabledText = "Positive", enabledText = "Negative")]
        public bool decreaseMass = true;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false, guiName = "Gravioli Core"), UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        public bool massEffectEnabled = false;

        [KSPAction("Toggle Core")]
        public void ToggleMassEffect(KSPActionParam param)
        {
            massEffectEnabled = !massEffectEnabled;
        }


        private float originalScaleFactor = 1;
        private bool originalDecreaseMass = false;
        private bool oldMassEffectEnabled = false;
        private float originalMass;


        public override void OnUpdate()
        {
            if (massEffectEnabled != oldMassEffectEnabled)
            {
                if (massEffectEnabled)
                {
                    originalMass = this.vessel.GetTotalMass();
                    if (decreaseMass)
                    {
                        foreach (Part p in this.vessel.Parts)
                        {
                            p.mass = p.mass / scaleFactor;
                        }
                    }
                    else
                    {
                        foreach (Part p in this.vessel.Parts)
                        {
                            p.mass = p.mass * scaleFactor;
                        }
                    }
                    oldMassEffectEnabled = true;
                    originalScaleFactor = scaleFactor;
                    originalDecreaseMass = decreaseMass;
                    ScreenMessages.PostScreenMessage("Gravioli Core is enabled.", 4f, ScreenMessageStyle.UPPER_CENTER);
                }
                else
                {
                    if (originalDecreaseMass)
                    {
                        foreach (Part p in this.vessel.Parts)
                        {
                            p.mass = p.mass * originalScaleFactor;
                        }
                    }
                    else
                    {
                        foreach (Part p in this.vessel.Parts)
                        {
                            p.mass = p.mass / originalScaleFactor;
                        }
                    }
                    oldMassEffectEnabled = false;
                    ScreenMessages.PostScreenMessage("Gravioli Core is disabled.", 4f, ScreenMessageStyle.UPPER_CENTER);
                }
            }
            if (massEffectEnabled)
            {
                // Don't allow those to be changed while the mass effect is active
                scaleFactor = originalScaleFactor;
                decreaseMass = originalDecreaseMass;

                this.part.AddThermalFlux(heatFactor * originalScaleFactor * originalMass);

                if (PartResourceLibrary.Instance.GetDefinition("ElectricCharge") != null)
                {
                    float cost = chargeFactor * originalScaleFactor * originalMass * Time.deltaTime;
                    if (this.part.RequestResource("ElectricCharge", cost) < cost)
                    {
                        massEffectEnabled = false;
                        ScreenMessages.PostScreenMessage("Not enough Electric Charge, shutting down Mass Effect Core.", 4f, ScreenMessageStyle.UPPER_CENTER);
                    }
                }
            }
            base.OnUpdate();
        }
    }
}