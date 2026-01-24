/**
 * Color Constants for Art Gallery Management System
 * 
 * This file exports color values that align with the Tailwind configuration.
 * These constants are used primarily for Chart.js and other JavaScript-based
 * visualizations that require hex color values instead of Tailwind classes.
 * 
 * All colors reference the Tailwind default palette to maintain consistency
 * with the utility classes used throughout the application.
 */

// Import Tailwind's default colors
import colors from 'tailwindcss/colors';

/**
 * Primary color palette (Blues) - from Tailwind config
 * Used for main UI elements, primary buttons, and key data points
 */
export const primaryColors = {
  50: '#f0f5ff',
  100: '#e0ebff',
  200: '#c7d9ff',
  300: '#a3bfff',
  400: '#7a9dff',
  500: '#5474f7',
  600: '#3d4eed',
  700: '#2f3bd4',
  800: '#2a34ab',
  900: '#283286',
  950: '#1a1f52',
};

/**
 * Secondary color palette (Amber/Gold) - from Tailwind config
 * Used for accents, highlights, and secondary data points
 */
export const secondaryColors = {
  50: '#fffbeb',
  100: '#fef3c7',
  200: '#fde68a',
  300: '#fcd34d',
  400: '#fbbf24',
  500: '#f59e0b',
  600: '#d97706',
  700: '#b45309',
  800: '#92400e',
  900: '#78350f',
  950: '#451a03',
};

/**
 * Gallery-specific colors - from Tailwind config
 * Brand colors specific to the gallery application
 */
export const galleryColors = {
  cream: '#faf7f2',
  charcoal: '#2d2d2d',
  burgundy: '#722f37',
  forest: '#2d5a27',
  slate: '#4a5568',
};

/**
 * Status colors - from Tailwind config
 * Used for ETL status, validation states, and alerts
 */
export const statusColors = {
  success: '#10b981',  // Emerald 500
  warning: '#f59e0b',  // Amber 500
  error: '#ef4444',    // Red 500
  info: '#3b82f6',     // Blue 500
};

/**
 * Chart color palette
 * Optimized set of distinct colors for data visualization
 * Uses Tailwind's default color palette for consistency
 */
export const chartColors = {
  blue: colors.blue[500],        // #3b82f6
  emerald: colors.emerald[500],  // #10b981
  amber: colors.amber[500],      // #f59e0b
  red: colors.red[500],          // #ef4444
  violet: colors.violet[500],    // #8b5cf6
  pink: colors.pink[500],        // #ec4899
  purple: colors.violet[600],    // #7c3aed
  orange: colors.orange[600],    // #ea580c
  green: colors.green[500],      // #22c55e
  yellow: colors.yellow[500],    // #eab308
  slate: colors.slate[400],      // #94a3b8
  cyan: colors.cyan[500],        // #06b6d4
  indigo: colors.indigo[500],    // #6366f1
};

/**
 * Pre-defined chart color arrays for multi-dataset charts
 */
export const chartColorPalettes = {
  // Standard 6-color palette for most charts
  standard: [
    chartColors.blue,
    chartColors.emerald,
    chartColors.amber,
    chartColors.red,
    chartColors.violet,
    chartColors.pink,
  ],
  
  // Extended palette for charts with more data series
  extended: [
    chartColors.blue,
    chartColors.emerald,
    chartColors.amber,
    chartColors.red,
    chartColors.violet,
    chartColors.pink,
    chartColors.cyan,
    chartColors.indigo,
    chartColors.yellow,
    chartColors.orange,
  ],
  
  // Purple-themed palette
  purple: [
    chartColors.purple,
    chartColors.orange,
    chartColors.green,
    chartColors.blue,
    chartColors.yellow,
    chartColors.pink,
  ],
  
  // Status-based palette
  status: [
    statusColors.success,
    statusColors.warning,
    statusColors.error,
    statusColors.info,
  ],
};

/**
 * KPI variant color mapping
 * Maps KPI card variants to their respective colors
 */
export const kpiVariantColors = {
  primary: chartColors.purple,    // #7c3aed
  secondary: chartColors.orange,  // #ea580c
  success: chartColors.green,     // #22c55e
  warning: chartColors.yellow,    // #eab308
  danger: chartColors.red,        // #ef4444
  info: chartColors.blue,         // #3b82f6
};

/**
 * Utility function to get a color from the chart palette by index
 * Cycles through colors if index exceeds palette length
 */
export const getChartColor = (index, palette = chartColorPalettes.standard) => {
  return palette[index % palette.length];
};

/**
 * Utility function to generate opacity variants of a color
 * Useful for creating background colors with transparency
 */
export const withOpacity = (hexColor, opacity) => {
  // Remove # if present
  const hex = hexColor.replace('#', '');
  
  // Parse hex to RGB
  const r = parseInt(hex.substring(0, 2), 16);
  const g = parseInt(hex.substring(2, 4), 16);
  const b = parseInt(hex.substring(4, 6), 16);
  
  return `rgba(${r}, ${g}, ${b}, ${opacity})`;
};

export default {
  primaryColors,
  secondaryColors,
  galleryColors,
  statusColors,
  chartColors,
  chartColorPalettes,
  kpiVariantColors,
  getChartColor,
  withOpacity,
};
