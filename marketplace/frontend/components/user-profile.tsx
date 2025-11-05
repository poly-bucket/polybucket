'use client';

import { useState } from 'react';
import { useAuth } from '@/context/AuthContext';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { 
  User, 
  Mail, 
  MapPin, 
  Globe, 
  Building, 
  Calendar, 
  Star, 
  Download, 
  MessageSquare,
  Crown,
  Shield,
  Code,
  LogOut
} from 'lucide-react';

export function UserProfile() {
  const { user, logout, isAdmin, isModerator, isDeveloper } = useAuth();
  const [isLoggingOut, setIsLoggingOut] = useState(false);

  if (!user) {
    return null;
  }

  const handleLogout = async () => {
    setIsLoggingOut(true);
    try {
      await logout();
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      setIsLoggingOut(false);
    }
  };

  const getRoleIcon = () => {
    if (isAdmin()) return <Crown className="h-4 w-4" />;
    if (isModerator()) return <Shield className="h-4 w-4" />;
    if (isDeveloper()) return <Code className="h-4 w-4" />;
    return <User className="h-4 w-4" />;
  };

  const getRoleColor = () => {
    if (isAdmin()) return 'bg-red-100 text-red-800 border-red-200';
    if (isModerator()) return 'bg-blue-100 text-blue-800 border-blue-200';
    if (isDeveloper()) return 'bg-green-100 text-green-800 border-green-200';
    return 'bg-gray-100 text-gray-800 border-gray-200';
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  return (
    <div className="space-y-6">
      {/* User Header */}
      <Card>
        <CardHeader>
          <div className="flex items-center gap-4">
            <Avatar className="h-16 w-16">
              <AvatarImage src={user.avatarUrl} alt={user.displayName || user.username} />
              <AvatarFallback className="text-lg">
                {(user.displayName || user.username).charAt(0).toUpperCase()}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1">
              <div className="flex items-center gap-2 mb-1">
                <h2 className="text-2xl font-bold">{user.displayName || user.username}</h2>
                {user.isVerified && (
                  <Badge variant="secondary" className="bg-blue-100 text-blue-800">
                    Verified
                  </Badge>
                )}
              </div>
              <div className="flex items-center gap-2 mb-2">
                {getRoleIcon()}
                <Badge className={getRoleColor()}>
                  {user.primaryRole}
                </Badge>
              </div>
              {user.bio && (
                <p className="text-muted-foreground">{user.bio}</p>
              )}
            </div>
            <Button 
              variant="outline" 
              onClick={handleLogout}
              disabled={isLoggingOut}
              className="gap-2"
            >
              <LogOut className="h-4 w-4" />
              {isLoggingOut ? 'Signing out...' : 'Sign out'}
            </Button>
          </div>
        </CardHeader>
      </Card>

      {/* User Stats */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Star className="h-5 w-5" />
            Statistics
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div className="text-center">
              <div className="text-2xl font-bold text-primary">{user.pluginCount}</div>
              <div className="text-sm text-muted-foreground">Plugins</div>
            </div>
            <div className="text-center">
              <div className="text-2xl font-bold text-primary">{user.installationCount}</div>
              <div className="text-sm text-muted-foreground">Installations</div>
            </div>
            <div className="text-center">
              <div className="text-2xl font-bold text-primary">{user.reviewCount}</div>
              <div className="text-sm text-muted-foreground">Reviews</div>
            </div>
            <div className="text-center">
              <div className="text-2xl font-bold text-primary">{user.reputationScore}</div>
              <div className="text-sm text-muted-foreground">Reputation</div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* User Details */}
      <div className="grid md:grid-cols-2 gap-6">
        {/* Contact Information */}
        <Card>
          <CardHeader>
            <CardTitle>Contact Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center gap-3">
              <Mail className="h-4 w-4 text-muted-foreground" />
              <span>{user.email}</span>
            </div>
            {user.location && (
              <div className="flex items-center gap-3">
                <MapPin className="h-4 w-4 text-muted-foreground" />
                <span>{user.location}</span>
              </div>
            )}
            {user.website && (
              <div className="flex items-center gap-3">
                <Globe className="h-4 w-4 text-muted-foreground" />
                <a 
                  href={user.website} 
                  target="_blank" 
                  rel="noopener noreferrer"
                  className="text-primary hover:underline"
                >
                  {user.website}
                </a>
              </div>
            )}
            {user.company && (
              <div className="flex items-center gap-3">
                <Building className="h-4 w-4 text-muted-foreground" />
                <span>{user.company}</span>
              </div>
            )}
            {user.githubProfileUrl && (
              <div className="flex items-center gap-3">
                <Code className="h-4 w-4 text-muted-foreground" />
                <a 
                  href={user.githubProfileUrl} 
                  target="_blank" 
                  rel="noopener noreferrer"
                  className="text-primary hover:underline"
                >
                  GitHub Profile
                </a>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Account Information */}
        <Card>
          <CardHeader>
            <CardTitle>Account Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center gap-3">
              <Calendar className="h-4 w-4 text-muted-foreground" />
              <div>
                <div className="text-sm font-medium">Member since</div>
                <div className="text-sm text-muted-foreground">
                  {formatDate(user.createdAt)}
                </div>
              </div>
            </div>
            {user.lastLoginAt && (
              <div className="flex items-center gap-3">
                <Calendar className="h-4 w-4 text-muted-foreground" />
                <div>
                  <div className="text-sm font-medium">Last login</div>
                  <div className="text-sm text-muted-foreground">
                    {formatDate(user.lastLoginAt)}
                  </div>
                </div>
              </div>
            )}
            <div className="flex items-center gap-3">
              <Shield className="h-4 w-4 text-muted-foreground" />
              <div>
                <div className="text-sm font-medium">Status</div>
                <Badge variant={user.status === 'active' ? 'default' : 'destructive'}>
                  {user.status}
                </Badge>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Roles and Permissions */}
      {(isAdmin() || isModerator()) && (
        <Card>
          <CardHeader>
            <CardTitle>Roles & Permissions</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <div className="text-sm font-medium mb-2">Roles</div>
              <div className="flex flex-wrap gap-2">
                {user.roles.map((role) => (
                  <Badge key={role} variant="outline">
                    {role}
                  </Badge>
                ))}
              </div>
            </div>
            <Separator />
            <div>
              <div className="text-sm font-medium mb-2">Permissions</div>
              <div className="flex flex-wrap gap-2">
                {user.permissions.slice(0, 10).map((permission) => (
                  <Badge key={permission} variant="secondary" className="text-xs">
                    {permission}
                  </Badge>
                ))}
                {user.permissions.length > 10 && (
                  <Badge variant="secondary" className="text-xs">
                    +{user.permissions.length - 10} more
                  </Badge>
                )}
              </div>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
