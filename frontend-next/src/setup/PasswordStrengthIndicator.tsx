interface PasswordStrengthIndicatorProps {
  password: string;
}

interface StrengthCriteria {
  label: string;
  met: boolean;
}

export default function PasswordStrengthIndicator({
  password,
}: PasswordStrengthIndicatorProps) {
  const criteria: StrengthCriteria[] = [
    { label: "At least 8 characters", met: password.length >= 8 },
    { label: "Contains uppercase letter", met: /[A-Z]/.test(password) },
    { label: "Contains lowercase letter", met: /[a-z]/.test(password) },
    { label: "Contains number", met: /\d/.test(password) },
    {
      label: "Contains special character",
      met: /[!@#$%^&*(),.?":{}|<>]/.test(password),
    },
  ];

  const calculateStrength = (): {
    score: number;
    label: string;
    color: string;
  } => {
    if (!password) {
      return { score: 0, label: "", color: "bg-gray-600" };
    }
    const metCriteria = criteria.filter((c) => c.met).length;
    const percentage = (metCriteria / criteria.length) * 100;
    if (percentage >= 80) {
      return { score: percentage, label: "Strong", color: "bg-green-500" };
    }
    if (percentage >= 60) {
      return { score: percentage, label: "Good", color: "bg-blue-500" };
    }
    if (percentage >= 40) {
      return { score: percentage, label: "Fair", color: "bg-yellow-500" };
    }
    return { score: percentage, label: "Weak", color: "bg-red-500" };
  };

  const strength = calculateStrength();

  if (!password) {
    return null;
  }

  return (
    <div className="space-y-3">
      <div>
        <div className="flex justify-between items-center mb-1">
          <span className="text-sm text-white/70">Password Strength</span>
          <span className="text-sm font-medium text-white">
            {strength.label}
          </span>
        </div>
        <div className="w-full bg-white/10 rounded-full h-2">
          <div
            className={`h-2 rounded-full transition-all duration-300 ${strength.color}`}
            style={{ width: `${strength.score}%` }}
          />
        </div>
      </div>
      <div className="space-y-1">
        <p className="text-xs text-white/50 mb-2">Password requirements:</p>
        {criteria.map((criterion, index) => (
          <div key={index} className="flex items-center space-x-2">
            <div
              className={`w-2 h-2 rounded-full ${
                criterion.met ? "bg-green-500" : "bg-white/20"
              }`}
            />
            <span
              className={`text-xs ${
                criterion.met ? "text-green-400" : "text-white/50"
              }`}
            >
              {criterion.label}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}
