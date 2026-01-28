export type MemberCategory = 'global' | 'site' | 'free';
export type UserRole = 'user' | 'admin_site' | 'admin_global';

export interface CurrentUser {
  subject: string;
  matricule: string;
  category: MemberCategory;
  role: UserRole;
  siteId: string | null;
}

export function isAdmin(user: CurrentUser | null): boolean {
  return user?.role === 'admin_site' || user?.role === 'admin_global';
}

export function isGlobalAdmin(user: CurrentUser | null): boolean {
  return user?.role === 'admin_global';
}

export function getBookingWindowDays(category: MemberCategory): number {
  switch (category) {
    case 'global':
      return 21;
    case 'site':
      return 14;
    case 'free':
      return 5;
  }
}
