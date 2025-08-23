using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace SolidAvalonia.Mixins;

/// <summary>
/// Extension methods for grid layout functionality
/// </summary>
public static class GridLayoutExtensions
{
    /// <summary>
    /// Creates a responsive grid layout
    /// </summary>
    public static Grid CreateGrid(this SolidControl control, string columnDefinitions = "*", string rowDefinitions = "*")
    {
        var grid = new Grid();

        // Parse column definitions
        var cols = columnDefinitions.Split(',');
        foreach (var col in cols)
        {
            var trimmed = col.Trim();
            if (trimmed == "*")
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            else if (trimmed.EndsWith("*"))
            {
                if (double.TryParse(trimmed.TrimEnd('*'), out var factor))
                    grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(factor, GridUnitType.Star)));
            }
            else if (trimmed.Equals("auto", StringComparison.OrdinalIgnoreCase))
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            else if (double.TryParse(trimmed, out var width))
                grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(width)));
        }

        // Parse row definitions
        var rows = rowDefinitions.Split(',');
        foreach (var row in rows)
        {
            var trimmed = row.Trim();
            if (trimmed == "*")
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            else if (trimmed.EndsWith("*"))
            {
                if (double.TryParse(trimmed.TrimEnd('*'), out var factor))
                    grid.RowDefinitions.Add(new RowDefinition(new GridLength(factor, GridUnitType.Star)));
            }
            else if (trimmed.Equals("auto", StringComparison.OrdinalIgnoreCase))
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            else if (double.TryParse(trimmed, out var height))
                grid.RowDefinitions.Add(new RowDefinition(new GridLength(height)));
        }

        return grid;
    }

    /// <summary>
    /// Helper to add a control to a grid at specific position
    /// </summary>
    public static T GridChild<T>(this SolidControl control, T gridChild, int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1)
        where T : Control
    {
        Grid.SetRow(gridChild, row);
        Grid.SetColumn(gridChild, column);
        Grid.SetRowSpan(gridChild, rowSpan);
        Grid.SetColumnSpan(gridChild, columnSpan);
        return gridChild;
    }
}