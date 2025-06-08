using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Menu.Remix.MixedUI;

namespace Downcast
{
    internal class DowncastOptions : OptionInterface
    {
        public static DowncastOptions instance = new DowncastOptions();
        public DowncastOptions()
        {
            dragCoefficient = this.config.Bind<float>("dragCoefficient", 31.25f, new ConfigAcceptableRange<float>(0f, 100f));
        }

        public readonly Configurable<float> dragCoefficient;
        public readonly Configurable<bool> fallOnOtherLayers;
        public readonly Configurable<bool> onlyPlayerFall;
        public readonly Configurable<bool> checkForConnections;

        private UIelement[] UIArrPlayerOptions;

        public override void Initialize()
        {
            var opTab = new OpTab(this, "Options");
            this.Tabs = new[]
            {
            opTab
        };

            UIArrPlayerOptions = new UIelement[]
            {
            new OpLabel(10f, 550f, "Physics Constants", true),
            new OpLabel(10f, 520f, "Drag Coefficient"),
            new OpFloatSlider(dragCoefficient, new Vector2(10f, 460f), 500),
            };
            opTab.AddItems(UIArrPlayerOptions);
        }
    }
}
