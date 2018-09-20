export class IConfirmModalDataModel {
  content = '';
  title? = '';
  textYesBtn? = '';
  yesAction?: () => void;
  closeAction?: () => void;
}
