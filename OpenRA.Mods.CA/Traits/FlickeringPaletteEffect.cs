﻿#region Copyright & License Information
/*
 * Copyright 2015- OpenRA.Mods.AS Developers (see AUTHORS)
 * This file is a part of a third-party plugin for OpenRA, which is
 * free software. It is made available to you under the terms of the
 * GNU General Public License as published by the Free Software
 * Foundation. For more information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using OpenRA.Graphics;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.CA.Traits
{
	public class FlickeringPaletteEffectInfo : TraitInfo
	{
		[Desc("The palette to apply this effect to.")]
		[PaletteReference]
		[FieldLoader.Require]
		public readonly string PaletteName;

		[Desc("Start colour.")]
		[FieldLoader.Require]
		public readonly Color StartColor;

		[Desc("End colour.")]
		[FieldLoader.Require]
		public readonly Color EndColor;

		[Desc("Number of ticks to pulse before reversing.")]
		public readonly int PulseDuration = 25;

		[Desc("Number of ticks to wait on start/end colours.")]
		public readonly int PulseDelay = 0;

		public readonly int? ShadowIndex = 4;

		public override object Create(ActorInitializer init) { return new FlickeringPaletteEffect(this); }
	}

	public class FlickeringPaletteEffect : IPaletteModifier, ITick
	{
		readonly FlickeringPaletteEffectInfo info;

		int ticks;
		bool incrementing;

		int redDiff;
		int greenDiff;
		int blueDiff;
		int alphaDiff;

		public FlickeringPaletteEffect(FlickeringPaletteEffectInfo info)
		{
			this.info = info;
			ticks = 0;
			incrementing = true;

			redDiff = (info.EndColor.R - info.StartColor.R) / info.PulseDuration;
			greenDiff = (info.EndColor.G - info.StartColor.G) / info.PulseDuration;
			blueDiff = (info.EndColor.B - info.StartColor.B) / info.PulseDuration;
			alphaDiff = (info.EndColor.A - info.StartColor.A) / info.PulseDuration;
		}

		public void AdjustPalette(IReadOnlyDictionary<string, MutablePalette> b)
		{
			var pulseTick = (ticks - info.PulseDelay).Clamp(0, info.PulseDuration);

			var red = (info.StartColor.R + (redDiff * pulseTick)).Clamp(0, 255);
			var green = (info.StartColor.G + (greenDiff * pulseTick)).Clamp(0, 255);
			var blue = (info.StartColor.B + (blueDiff * pulseTick)).Clamp(0, 255);
			var alpha = (info.StartColor.A + (alphaDiff * pulseTick)).Clamp(0, 255);

			var p = b[info.PaletteName];

			for (int j = 1; j < Palette.Size; j++)
			{
				if (info.ShadowIndex != null && info.ShadowIndex == j)
					continue;

				var color = Color.FromLinear((byte)alpha, red / 255f, green / 255f, blue / 255f);
				p.SetColor(j, color);
			}
		}

		void ITick.Tick(Actor self)
		{
			if (incrementing)
				ticks++;
			else
				ticks--;

			if (ticks == info.PulseDuration + (info.PulseDelay * 2) || ticks == 0)
				incrementing = !incrementing;
		}
	}
}
