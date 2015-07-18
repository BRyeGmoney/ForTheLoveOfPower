#if !IgnoreHexLib
//----------------------------------------------//
// Gamelogic Grids                              //
// http://www.gamelogic.co.za                   //
// Copyright (c) 2013 Gamelogic (Pty) Ltd       //
//----------------------------------------------//

// Auto-generated File

using System;

namespace Gamelogic.Grids
{
	public partial class RectGrid<TCell>
	{
		/**
			\copydoc RectOp<TCell>::FixedWidth
		*/
		public static RectGrid<TCell> FixedWidth(Int32 width, Int32 cellCount)
		{
			return BeginShape().FixedWidth(width, cellCount).EndShape();
		}

		/**
			\copydoc RectOp<TCell>::FixedHeight
		*/
		public static RectGrid<TCell> FixedHeight(Int32 height, Int32 cellCount)
		{
			return BeginShape().FixedHeight(height, cellCount).EndShape();
		}

		/**
			\copydoc RectOp<TCell>::Rectangle
		*/
		public static RectGrid<TCell> Rectangle(Int32 width, Int32 height)
		{
			return BeginShape().Rectangle(width, height).EndShape();
		}

		/**
			\copydoc RectOp<TCell>::Circle
		*/
		public static RectGrid<TCell> Circle(Int32 radius)
		{
			return BeginShape().Circle(radius).EndShape();
		}

		/**
			\copydoc RectOp<TCell>::Parallelogram
		*/
		public static RectGrid<TCell> Parallelogram(Int32 width, Int32 height)
		{
			return BeginShape().Parallelogram(width, height).EndShape();
		}

		/**
			\copydoc RectOp<TCell>::CheckerBoard
		*/
		public static RectGrid<TCell> CheckerBoard(Int32 width, Int32 height)
		{
			return BeginShape().CheckerBoard(width, height).EndShape();
		}

		/**
			\copydoc RectOp<TCell>::CheckerBoard
		*/
		public static RectGrid<TCell> CheckerBoard(Int32 width, Int32 height, Boolean includesOrigin)
		{
			return BeginShape().CheckerBoard(width, height, includesOrigin).EndShape();
		}

		/**
			\copydoc RectOp<TCell>::Default
		*/
		public static RectGrid<TCell> Default(Int32 width, Int32 height)
		{
			return BeginShape().Default(width, height).EndShape();
		}

		/**
			\copydoc RectOp<TCell>::Single
		*/
		public static RectGrid<TCell> Single()
		{
			return BeginShape().Single().EndShape();
		}

	}
	public partial class DiamondGrid<TCell>
	{
		/**
			\copydoc DiamondOp<TCell>::Diamond
		*/
		public static DiamondGrid<TCell> Diamond(Int32 side)
		{
			return BeginShape().Diamond(side).EndShape();
		}

		/**
			\copydoc DiamondOp<TCell>::Parallelogram
		*/
		public static DiamondGrid<TCell> Parallelogram(Int32 width, Int32 height)
		{
			return BeginShape().Parallelogram(width, height).EndShape();
		}

		/**
			\copydoc DiamondOp<TCell>::Rectangle
		*/
		public static DiamondGrid<TCell> Rectangle(Int32 width, Int32 height)
		{
			return BeginShape().Rectangle(width, height).EndShape();
		}

		/**
			\copydoc DiamondOp<TCell>::ThinRectangle
		*/
		public static DiamondGrid<TCell> ThinRectangle(Int32 width, Int32 height)
		{
			return BeginShape().ThinRectangle(width, height).EndShape();
		}

		/**
			\copydoc DiamondOp<TCell>::FatRectangle
		*/
		public static DiamondGrid<TCell> FatRectangle(Int32 width, Int32 height)
		{
			return BeginShape().FatRectangle(width, height).EndShape();
		}

		/**
			\copydoc DiamondOp<TCell>::Default
		*/
		public static DiamondGrid<TCell> Default(Int32 width, Int32 height)
		{
			return BeginShape().Default(width, height).EndShape();
		}

		/**
			\copydoc DiamondOp<TCell>::Single
		*/
		public static DiamondGrid<TCell> Single()
		{
			return BeginShape().Single().EndShape();
		}

	}
	public partial class PointyHexGrid<TCell>
	{
		/**
			\copydoc PointyHexOp<TCell>::Rectangle
		*/
		public static PointyHexGrid<TCell> Rectangle(Int32 width, Int32 height)
		{
			return BeginShape().Rectangle(width, height).EndShape();
		}

		/**
			\copydoc PointyHexOp<TCell>::Hexagon
		*/
		public static PointyHexGrid<TCell> Hexagon(Int32 side)
		{
			return BeginShape().Hexagon(side).EndShape();
		}

		/**
			\copydoc PointyHexOp<TCell>::Hexagon
		*/
		public static PointyHexGrid<TCell> Hexagon(PointyHexPoint centre, Int32 side)
		{
			return BeginShape().Hexagon(centre, side).EndShape();
		}

		/**
			\copydoc PointyHexOp<TCell>::Parallelogram
		*/
		public static PointyHexGrid<TCell> Parallelogram(Int32 width, Int32 height)
		{
			return BeginShape().Parallelogram(width, height).EndShape();
		}

		/**
			\copydoc PointyHexOp<TCell>::UpTriangle
		*/
		public static PointyHexGrid<TCell> UpTriangle(Int32 side)
		{
			return BeginShape().UpTriangle(side).EndShape();
		}

		/**
			\copydoc PointyHexOp<TCell>::DownTriangle
		*/
		public static PointyHexGrid<TCell> DownTriangle(Int32 side)
		{
			return BeginShape().DownTriangle(side).EndShape();
		}

		/**
			\copydoc PointyHexOp<TCell>::Diamond
		*/
		public static PointyHexGrid<TCell> Diamond(Int32 side)
		{
			return BeginShape().Diamond(side).EndShape();
		}

		/**
			\copydoc PointyHexOp<TCell>::ThinRectangle
		*/
		public static PointyHexGrid<TCell> ThinRectangle(Int32 width, Int32 height)
		{
			return BeginShape().ThinRectangle(width, height).EndShape();
		}

		/**
			\copydoc PointyHexOp<TCell>::FatRectangle
		*/
		public static PointyHexGrid<TCell> FatRectangle(Int32 width, Int32 height)
		{
			return BeginShape().FatRectangle(width, height).EndShape();
		}

		/**
			\copydoc PointyHexOp<TCell>::Default
		*/
		public static PointyHexGrid<TCell> Default(Int32 width, Int32 height)
		{
			return BeginShape().Default(width, height).EndShape();
		}

		/**
			\copydoc PointyHexOp<TCell>::Single
		*/
		public static PointyHexGrid<TCell> Single()
		{
			return BeginShape().Single().EndShape();
		}

	}
	public partial class FlatHexGrid<TCell>
	{
		/**
			\copydoc FlatHexOp<TCell>::Rectangle
		*/
		public static FlatHexGrid<TCell> Rectangle(Int32 width, Int32 height)
		{
			return BeginShape().Rectangle(width, height).EndShape();
		}

		/**
			\copydoc FlatHexOp<TCell>::FatRectangle
		*/
		public static FlatHexGrid<TCell> FatRectangle(Int32 width, Int32 height)
		{
			return BeginShape().FatRectangle(width, height).EndShape();
		}

		/**
			\copydoc FlatHexOp<TCell>::ThinRectangle
		*/
		public static FlatHexGrid<TCell> ThinRectangle(Int32 width, Int32 height)
		{
			return BeginShape().ThinRectangle(width, height).EndShape();
		}

		/**
			\copydoc FlatHexOp<TCell>::Hexagon
		*/
		public static FlatHexGrid<TCell> Hexagon(Int32 side)
		{
			return BeginShape().Hexagon(side).EndShape();
		}

		/**
			\copydoc FlatHexOp<TCell>::Hexagon
		*/
		public static FlatHexGrid<TCell> Hexagon(FlatHexPoint centre, Int32 side)
		{
			return BeginShape().Hexagon(centre, side).EndShape();
		}

		/**
			\copydoc FlatHexOp<TCell>::LeftTriangle
		*/
		public static FlatHexGrid<TCell> LeftTriangle(Int32 side)
		{
			return BeginShape().LeftTriangle(side).EndShape();
		}

		/**
			\copydoc FlatHexOp<TCell>::RightTriangle
		*/
		public static FlatHexGrid<TCell> RightTriangle(Int32 side)
		{
			return BeginShape().RightTriangle(side).EndShape();
		}

		/**
			\copydoc FlatHexOp<TCell>::Parallelogram
		*/
		public static FlatHexGrid<TCell> Parallelogram(Int32 width, Int32 height)
		{
			return BeginShape().Parallelogram(width, height).EndShape();
		}

		/**
			\copydoc FlatHexOp<TCell>::Diamond
		*/
		public static FlatHexGrid<TCell> Diamond(Int32 side)
		{
			return BeginShape().Diamond(side).EndShape();
		}

		/**
			\copydoc FlatHexOp<TCell>::Default
		*/
		public static FlatHexGrid<TCell> Default(Int32 width, Int32 height)
		{
			return BeginShape().Default(width, height).EndShape();
		}

		/**
			\copydoc FlatHexOp<TCell>::Single
		*/
		public static FlatHexGrid<TCell> Single()
		{
			return BeginShape().Single().EndShape();
		}

	}
}
#endif
