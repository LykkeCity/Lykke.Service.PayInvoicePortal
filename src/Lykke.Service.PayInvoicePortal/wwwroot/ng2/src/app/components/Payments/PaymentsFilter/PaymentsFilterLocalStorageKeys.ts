import { UserInfoModel } from 'src/app/models/UserInfoModel';

export class PaymentsFilterLocalStorageKeys {
  private static readonly PeriodKey: string = 'PaymentsFilter_Period';
  private static readonly TypeKey: string = 'PaymentsFilter_Type';
  private static readonly StatusKey: string = 'PaymentsFilter_Status';
  private static readonly SearchTextKey: string = 'PaymentsFilter_SearchText';

  static Period(user: UserInfoModel): string {
    return `${PaymentsFilterLocalStorageKeys.PeriodKey}_${user.employeeId}`;
  }

  static Type(user: UserInfoModel): string {
    return `${PaymentsFilterLocalStorageKeys.TypeKey}_${user.employeeId}`;
  }

  static Status(user: UserInfoModel): string {
    return `${PaymentsFilterLocalStorageKeys.StatusKey}_${user.employeeId}`;
  }

  static SearchText(user: UserInfoModel): string {
    return `${PaymentsFilterLocalStorageKeys.SearchTextKey}_${user.employeeId}`;
  }
}
