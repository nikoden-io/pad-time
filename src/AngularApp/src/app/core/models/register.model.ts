// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
/** Request payload for new user registration. */
﻿export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
}

/** Response returned after successful user registration. */
export interface RegisterResponse {
  matricule: string;
  email: string;
}