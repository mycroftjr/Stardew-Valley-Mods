﻿using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Shockah.CommonModCode;
using Shockah.CommonModCode.UI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Shockah.MachineStatus
{
	internal class ModConfig
	{
		public bool IgnoreUnknownItems { get; set; } = true;

		public UIAnchorSide ScreenAnchorSide { get; set; } = UIAnchorSide.BottomLeft;
		public float AnchorInset { get; set; } = 16f;
		public float AnchorOffsetX { get; set; } = 0f;
		public float AnchorOffsetY { get; set; } = 0f;
		public UIAnchorSide PanelAnchorSide { get; set; } = UIAnchorSide.BottomLeft;
		[JsonIgnore] public UIAnchor Anchor => new(ScreenAnchorSide, AnchorInset, new(AnchorOffsetX, AnchorOffsetY), PanelAnchorSide);

		public FlowDirection FlowDirection { get; set; } = FlowDirection.LeftToRightAndBottomToTop;
		public float Scale { get; set; } = 2f;
		public float XSpacing { get; set; } = 4f;
		public float YSpacing { get; set; } = 4f;
		[JsonIgnore] public Vector2 Spacing => new(XSpacing, YSpacing);
		public int MaxColumns { get; set; } = 6;

		public bool ShowItemBubble { get; set; } = true;
		public float BubbleItemCycleTime { get; set; } = 2f;
		public MachineRenderingOptions.BubbleSway BubbleSway { get; set; } = MachineRenderingOptions.BubbleSway.Wave;

		public SplitScreenScreens SplitScreenScreens { get; set; } = SplitScreenScreens.First;
		public KeybindList VisibilityKeybind { get; set; } = new KeybindList();
		public float FocusedAlpha { get; set; } = 1f;
		public float NormalAlpha { get; set; } = 0.3f;
		public bool BusyDancing { get; set; } = true;

		public MachineRenderingOptions.Grouping Grouping { get; set; } = MachineRenderingOptions.Grouping.ByMachine;
		public IList<MachineRenderingOptions.Sorting> Sorting { get; set; } = new List<MachineRenderingOptions.Sorting>
		{
			MachineRenderingOptions.Sorting.ReadyFirst,
			MachineRenderingOptions.Sorting.WaitingFirst,
			MachineRenderingOptions.Sorting.ByMachineAZ,
			MachineRenderingOptions.Sorting.ByItemAZ
		};

		public bool ShowReady { get; set; } = true;
		public IList<string> ShowReadyExceptions { get; set; } = new List<string>();

		public bool ShowWaiting { get; set; } = false;
		public IList<string> ShowWaitingExceptions { get; set; } = new List<string> { "*|Cask", "*|Keg", "*|Preserves Jar", "*|Crab Pot" };

		public bool ShowBusy { get; set; } = false;
		public IList<string> ShowBusyExceptions { get; set; } = new List<string>();

		[JsonIgnore]
		public IReadOnlyList<IWildcardPattern> ShowReadyExceptionPatterns
		{
			get => ShowReadyExceptions.Select(WildcardPatterns.Parse).ToList();
			set => ShowReadyExceptions = value.Select(p => p.Pattern).ToList();
		}

		[JsonIgnore]
		public IReadOnlyList<IWildcardPattern> ShowWaitingExceptionPatterns
		{
			get => ShowWaitingExceptions.Select(WildcardPatterns.Parse).ToList();
			set => ShowWaitingExceptions = value.Select(p => p.Pattern).ToList();
		}

		[JsonIgnore]
		public IReadOnlyList<IWildcardPattern> ShowBusyExceptionPatterns
		{
			get => ShowBusyExceptions.Select(WildcardPatterns.Parse).ToList();
			set => ShowBusyExceptions = value.Select(p => p.Pattern).ToList();
		}
	}
}
