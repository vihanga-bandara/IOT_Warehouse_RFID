// ToolTrackPro Brand Colors & Theme Configuration
export const theme = {
  colors: {
    // Primary Colors (Blue from logo)
    primaryDark: '#003d6b',
    primaryLight: '#1e90ff',
    
    // Accent Colors
    accentGreen: '#50c878',
    accentGray: '#b8c5d6',
    
    // Text Colors
    darkText: '#1a2332',
    lightText: '#ffffff',
    
    // Utility Colors
    borderColor: '#e1e8ed',
    bgLight: '#f8f9fa',
    bgLighter: '#f0f4f8',
    errorColor: '#e74c3c',
    successColor: '#27ae60',
    warningColor: '#f39c12',
    infoColor: '#3498db',
  },
  
  gradients: {
    primary: 'linear-gradient(135deg, #003d6b 0%, #1e90ff 100%)',
    accent: 'linear-gradient(135deg, #1e90ff 50%, #50c878 100%)',
    full: 'linear-gradient(135deg, #003d6b 0%, #1e90ff 50%, #50c878 100%)',
  },
  
  shadows: {
    sm: '0 2px 8px rgba(0, 61, 107, 0.1)',
    md: '0 4px 12px rgba(30, 144, 255, 0.2)',
    lg: '0 8px 24px rgba(0, 61, 107, 0.15)',
    xl: '0 20px 60px rgba(0, 61, 107, 0.25)',
  },
  
  borderRadius: {
    xs: '4px',
    sm: '6px',
    md: '10px',
    lg: '16px',
    full: '9999px',
  },
  
  spacing: {
    xs: '0.25rem',
    sm: '0.5rem',
    md: '1rem',
    lg: '1.5rem',
    xl: '2rem',
    '2xl': '3rem',
  },
}

export default theme
