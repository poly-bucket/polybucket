import { createTheme } from '@mui/material/styles';

export const liquidGlassTheme = createTheme({
  palette: {
    mode: 'dark',
    primary: {
      main: '#6366f1',
      light: '#818cf8',
      dark: '#4f46e5',
    },
    secondary: {
      main: '#8b5cf6',
      light: '#a78bfa',
      dark: '#7c3aed',
    },
    background: {
      default: '#0f0f23',
      paper: 'rgba(255, 255, 255, 0.08)',
    },
    text: {
      primary: '#ffffff',
      secondary: 'rgba(255, 255, 255, 0.8)',
    },
    error: {
      main: '#ef4444',
    },
    warning: {
      main: '#f59e0b',
    },
    info: {
      main: '#3b82f6',
    },
    success: {
      main: '#10b981',
    },
  },
  typography: {
    fontFamily: 'Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
    h1: {
      color: 'var(--lg-text-primary)',
    },
    h2: {
      color: 'var(--lg-text-primary)',
    },
    h3: {
      color: 'var(--lg-text-primary)',
    },
    h4: {
      color: 'var(--lg-text-primary)',
    },
    h5: {
      color: 'var(--lg-text-primary)',
    },
    h6: {
      color: 'var(--lg-text-primary)',
    },
    body1: {
      color: 'var(--lg-text-secondary)',
    },
    body2: {
      color: 'var(--lg-text-tertiary)',
    },
  },
  components: {
    MuiCssBaseline: {
      styleOverrides: {
        body: {
          backgroundColor: 'var(--lg-bg-primary)',
          color: 'var(--lg-text-primary)',
          margin: 0,
          padding: 0,
          fontFamily: 'Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
        },
        '#root': {
          minHeight: '100vh',
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          background: 'rgba(255, 255, 255, 0.08)',
          backdropFilter: 'blur(20px)',
          WebkitBackdropFilter: 'blur(20px)',
          border: '1px solid rgba(255, 255, 255, 0.12)',
          borderRadius: '0.75rem',
          boxShadow: '0 8px 32px rgba(0, 0, 0, 0.3)',
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          background: 'rgba(255, 255, 255, 0.08)',
          backdropFilter: 'blur(20px)',
          WebkitBackdropFilter: 'blur(20px)',
          border: '1px solid rgba(255, 255, 255, 0.12)',
          borderRadius: '0.75rem',
          boxShadow: '0 8px 32px rgba(0, 0, 0, 0.3)',
          transition: 'all 250ms ease-in-out',
          '&:hover': {
            transform: 'translateY(-2px)',
            boxShadow: '0 12px 40px rgba(0, 0, 0, 0.4)',
            borderColor: 'rgba(99, 102, 241, 0.3)',
          },
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: {
          background: 'rgba(255, 255, 255, 0.08)',
          backdropFilter: 'blur(20px)',
          WebkitBackdropFilter: 'blur(20px)',
          border: '1px solid rgba(255, 255, 255, 0.12)',
          borderRadius: '0.5rem',
          color: '#ffffff',
          padding: '0.5rem 1.5rem',
          fontWeight: 500,
          fontSize: '0.875rem',
          transition: 'all 150ms ease-in-out',
          minHeight: '2.5rem',
          textTransform: 'none',
          '&:hover': {
            background: 'rgba(255, 255, 255, 0.1)',
            borderColor: 'rgba(99, 102, 241, 0.3)',
            transform: 'translateY(-1px)',
            boxShadow: '0 4px 20px rgba(0, 0, 0, 0.3)',
          },
          '&:active': {
            background: 'rgba(255, 255, 255, 0.15)',
            transform: 'translateY(0)',
          },
          '&:disabled': {
            background: 'rgba(255, 255, 255, 0.05)',
            color: 'rgba(255, 255, 255, 0.4)',
          },
        },
        contained: {
          '&.MuiButton-containedPrimary': {
            background: 'linear-gradient(135deg, #6366f1 0%, #4f46e5 100%)',
            borderColor: '#6366f1',
            color: 'white',
            '&:hover': {
              background: 'linear-gradient(135deg, #818cf8 0%, #6366f1 100%)',
              borderColor: '#818cf8',
            },
          },
          '&.MuiButton-containedSecondary': {
            background: 'linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%)',
            borderColor: '#8b5cf6',
            color: 'white',
            '&:hover': {
              background: 'linear-gradient(135deg, #a78bfa 0%, #8b5cf6 100%)',
              borderColor: '#a78bfa',
            },
          },
        },
      },
    },
    MuiTextField: {
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-root': {
            background: 'rgba(255, 255, 255, 0.08)',
            backdropFilter: 'blur(20px)',
            WebkitBackdropFilter: 'blur(20px)',
            border: '1px solid rgba(255, 255, 255, 0.12)',
            borderRadius: '0.5rem',
            color: '#ffffff',
            '& fieldset': {
              border: 'none',
            },
            '&:hover fieldset': {
              border: 'none',
            },
            '&.Mui-focused fieldset': {
              border: 'none',
            },
            '&.Mui-focused': {
              borderColor: 'rgba(99, 102, 241, 0.3)',
              boxShadow: '0 0 0 3px rgba(99, 102, 241, 0.1)',
              background: 'rgba(255, 255, 255, 0.12)',
            },
          },
          '& .MuiInputLabel-root': {
            color: 'rgba(255, 255, 255, 0.4)',
            '&.Mui-focused': {
              color: '#ffffff',
            },
          },
        },
      },
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          background: 'var(--lg-glass-bg)',
          backdropFilter: 'var(--lg-glass-blur)',
          WebkitBackdropFilter: 'var(--lg-glass-blur)',
          borderBottom: '1px solid var(--lg-glass-border)',
          boxShadow: 'none',
        },
      },
    },
    MuiDrawer: {
      styleOverrides: {
        paper: {
          background: 'var(--lg-glass-bg)',
          backdropFilter: 'var(--lg-glass-blur)',
          WebkitBackdropFilter: 'var(--lg-glass-blur)',
          borderRight: '1px solid var(--lg-glass-border)',
        },
      },
    },
    MuiDialog: {
      styleOverrides: {
        paper: {
          background: 'var(--lg-glass-bg)',
          backdropFilter: 'var(--lg-glass-blur)',
          WebkitBackdropFilter: 'var(--lg-glass-blur)',
          border: '1px solid var(--lg-glass-border)',
          borderRadius: 'var(--lg-radius-xl)',
          boxShadow: 'var(--lg-glass-shadow)',
        },
      },
    },
    MuiBackdrop: {
      styleOverrides: {
        root: {
          background: 'var(--lg-bg-overlay)',
          backdropFilter: 'blur(10px)',
          WebkitBackdropFilter: 'blur(10px)',
        },
      },
    },
    MuiTableHead: {
      styleOverrides: {
        root: {
          '& .MuiTableCell-head': {
            background: 'var(--lg-glass-bg)',
            backdropFilter: 'var(--lg-glass-blur)',
            WebkitBackdropFilter: 'var(--lg-glass-blur)',
            fontWeight: 600,
            color: 'var(--lg-text-primary)',
            borderBottom: '1px solid var(--lg-glass-border)',
          },
        },
      },
    },
    MuiTableCell: {
      styleOverrides: {
        root: {
          borderBottom: '1px solid var(--lg-glass-border)',
          color: 'var(--lg-text-secondary)',
        },
      },
    },
    MuiTableRow: {
      styleOverrides: {
        root: {
          '&:hover': {
            background: 'var(--lg-button-hover)',
          },
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: {
          background: 'var(--lg-glass-bg)',
          backdropFilter: 'var(--lg-glass-blur)',
          WebkitBackdropFilter: 'var(--lg-glass-blur)',
          border: '1px solid var(--lg-glass-border)',
          color: 'var(--lg-text-secondary)',
        },
      },
    },
    MuiAlert: {
      styleOverrides: {
        root: {
          background: 'var(--lg-glass-bg)',
          backdropFilter: 'var(--lg-glass-blur)',
          WebkitBackdropFilter: 'var(--lg-glass-blur)',
          border: '1px solid var(--lg-glass-border)',
        },
      },
    },
  },
}); 