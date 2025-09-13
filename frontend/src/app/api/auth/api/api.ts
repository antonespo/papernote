export * from './auth.service';
import { AuthService } from './auth.service';
export * from './userResolution.service';
import { UserResolutionService } from './userResolution.service';
export const APIS = [AuthService, UserResolutionService];
