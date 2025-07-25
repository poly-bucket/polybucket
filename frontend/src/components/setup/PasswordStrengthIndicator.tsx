import React from 'react';

interface PasswordStrengthIndicatorProps {
  password: string;
}

interface StrengthCriteria {
  label: string;
  test: (password: string) => boolean;
  met: boolean;
}

const PasswordStrengthIndicator: React.FC<PasswordStrengthIndicatorProps> = ({ password }) => {
  const criteria: StrengthCriteria[] = [
    {
      label: 'At least 8 characters',
      test: (pwd) => pwd.length >= 8,
      met: password.length >= 8
    },
    {
      label: 'Contains uppercase letter',
      test: (pwd) => /[A-Z]/.test(pwd),
      met: /[A-Z]/.test(password)
    },
    {
      label: 'Contains lowercase letter',
      test: (pwd) => /[a-z]/.test(pwd),
      met: /[a-z]/.test(password)
    },
    {
      label: 'Contains number',
      test: (pwd) => /\d/.test(pwd),
      met: /\d/.test(password)
    },
    {
      label: 'Contains special character',
      test: (pwd) => /[!@#$%^&*(),.?":{}|<>]/.test(pwd),
      met: /[!@#$%^&*(),.?":{}|<>]/.test(password)
    }
  ];

  const calculateStrength = (): { score: number; label: string; color: string } => {
    if (!password) {
      return { score: 0, label: '', color: 'bg-gray-600' };
    }

    const metCriteria = criteria.filter(c => c.met).length;
    const totalCriteria = criteria.length;
    const percentage = (metCriteria / totalCriteria) * 100;

    if (percentage >= 80) {
      return { score: percentage, label: 'Strong', color: 'bg-green-500' };
    } else if (percentage >= 60) {
      return { score: percentage, label: 'Good', color: 'bg-blue-500' };
    } else if (percentage >= 40) {
      return { score: percentage, label: 'Fair', color: 'bg-yellow-500' };
    } else {
      return { score: percentage, label: 'Weak', color: 'bg-red-500' };
    }
  };

  const strength = calculateStrength();

  if (!password) {
    return null;
  }

  return (
    <div className="space-y-3">
      <div>
        <div className="flex justify-between items-center mb-1">
          <span className="text-sm text-gray-300">Password Strength</span>
          <span className="text-sm font-medium text-white">{strength.label}</span>
        </div>
        <div className="w-full bg-gray-700 rounded-full h-2">
          <div
            className={`h-2 rounded-full transition-all duration-300 ${strength.color}`}
            style={{ width: `${strength.score}%` }}
          />
        </div>
      </div>

      <div className="space-y-1">
        <p className="text-xs text-gray-400 mb-2">Password requirements:</p>
        {criteria.map((criterion, index) => (
          <div key={index} className="flex items-center space-x-2">
            <div className={`w-2 h-2 rounded-full ${criterion.met ? 'bg-green-500' : 'bg-gray-600'}`} />
            <span className={`text-xs ${criterion.met ? 'text-green-400' : 'text-gray-400'}`}>
              {criterion.label}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
};

export default PasswordStrengthIndicator; 