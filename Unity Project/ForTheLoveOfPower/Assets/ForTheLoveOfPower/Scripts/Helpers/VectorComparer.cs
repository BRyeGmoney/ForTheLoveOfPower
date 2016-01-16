using System.Collections.Generic;
using Gamelogic.Grids;
using UnityEngine;

    /// <summary>
    ///     ClockwiseComparer provides functionality for sorting a collection of Vector2s such
    ///     that they are ordered clockwise about a given origin.
    /// </summary>
    public class ClockwiseComparer : IComparer<Vector2>
    {
        private Vector2 m_Origin;

        #region Properties

        /// <summary>
        ///     Gets or sets the origin.
        /// </summary>
        /// <value>The origin.</value>
        public Vector2 origin { get { return m_Origin; } set { m_Origin = value; } }

        #endregion

        /// <summary>
        ///     Initializes a new instance of the ClockwiseComparer class.
        /// </summary>
        /// <param name="origin">Origin.</param>
        public ClockwiseComparer(Vector2 origin)
        {
            m_Origin = origin;
        }

        #region IComparer Methods

        /// <summary>
        ///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="first">First.</param>
        /// <param name="second">Second.</param>
        public int Compare(Vector2 first, Vector2 second)
        {
            return IsClockwise(first, second, m_Origin);
        }

        #endregion

        /// <summary>
        ///     Returns 1 if first comes before second in clockwise order.
        ///     Returns -1 if second comes before first.
        ///     Returns 0 if the points are identical.
        /// </summary>
        /// <param name="first">First.</param>
        /// <param name="second">Second.</param>
        /// <param name="origin">Origin.</param>
        public static int IsClockwise(Vector2 first, Vector2 second, Vector2 origin)
        {
            if (first == second)
                return 0;

            Vector2 firstOffset = first - origin;
            Vector2 secondOffset = second - origin;

            float angle1 = Mathf.Atan2(firstOffset.x, firstOffset.y);
            float angle2 = Mathf.Atan2(secondOffset.x, secondOffset.y);

            if (angle1 > angle2)
                return -1;

            if (angle1 < angle2)
                return 1;

            // Check to see which point is closest
            return (firstOffset.sqrMagnitude < secondOffset.sqrMagnitude) ? 1 : -1;
        }
    }


/// <summary>
///     ClockwiseComparer provides functionality for sorting a collection of Vector2s such
///     that they are ordered clockwise about a given origin.
/// </summary>
public class ClockwisePointyHexComparer : IComparer<PointyHexPoint>
{
    private PointyHexPoint m_Origin;

    #region Properties

    /// <summary>
    ///     Gets or sets the origin.
    /// </summary>
    /// <value>The origin.</value>
    public PointyHexPoint origin { get { return m_Origin; } set { m_Origin = value; } }

    #endregion

    /// <summary>
    ///     Initializes a new instance of the ClockwiseComparer class.
    /// </summary>
    /// <param name="origin">Origin.</param>
    public ClockwisePointyHexComparer(PointyHexPoint origin)
    {
        m_Origin = origin;
    }

    #region IComparer Methods

    /// <summary>
    ///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <param name="first">First.</param>
    /// <param name="second">Second.</param>
    public int Compare(PointyHexPoint first, PointyHexPoint second)
    {
        return IsClockwise(first, second, m_Origin);
    }

    #endregion

    /// <summary>
    ///     Returns 1 if first comes before second in clockwise order.
    ///     Returns -1 if second comes before first.
    ///     Returns 0 if the points are identical.
    /// </summary>
    /// <param name="first">First.</param>
    /// <param name="second">Second.</param>
    /// <param name="origin">Origin.</param>
    public static int IsClockwise(PointyHexPoint first, PointyHexPoint second, PointyHexPoint origin)
    {
        if (first == second)
            return 0;

        //if (second.Y < first.Y)
        //    return -1;
        //else if (second.Y > first.Y)
        //    return 1;
        //else
        //{
        //    if (second.X < first.X)
        //        return -1;
        //    else
        //        return 1;
        //}

        PointyHexPoint firstOffset = first - origin;
        PointyHexPoint secondOffset = second - origin;

        float angle1 = Mathf.Atan2(firstOffset.X, firstOffset.Y);
        float angle2 = Mathf.Atan2(secondOffset.X, secondOffset.Y);

        if (angle1 > angle2)
            return -1;

        if (angle1 < angle2)
            return 1;

        // Check to see which point is closest
        return (Vector2.SqrMagnitude(new Vector2(firstOffset.X, firstOffset.Y)) < Vector2.SqrMagnitude(new Vector2(secondOffset.X, secondOffset.Y))) ? 1 : -1;

    }
}