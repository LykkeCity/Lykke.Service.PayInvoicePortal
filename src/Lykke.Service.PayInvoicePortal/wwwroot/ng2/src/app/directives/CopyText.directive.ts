import { Directive, Input, HostListener, ElementRef } from '@angular/core';
declare const $: any;

@Directive({
  selector: '[lpCopyText]'
})
export class CopyTextDirective {
  @Input('lpCopyText')
  text: string;

  @Input('lpCopyTextTitle')
  title?: string;
  private tooltipTimeout: any;

  constructor(private element: ElementRef) {}

  @HostListener('click', ['$event.target'])
  public onClick(event: Event) {
    this.copyText(this.text);
    const tooltip = $(this.element.nativeElement).tooltip({
      title: this.title || 'Copied',
      placement: 'top',
      trigger: 'manual'
    });
    tooltip.tooltip('show');

    if (this.tooltipTimeout) {
      clearTimeout(this.tooltipTimeout);
    }

    this.tooltipTimeout = setTimeout(() => {
      tooltip.tooltip('hide');
    }, 3000);
  }

  private copyText(text: string) {
    const textarea = document.createElement('textarea');
    textarea.style.position = 'fixed';
    textarea.style.left = '0';
    textarea.style.top = '0';
    textarea.style.opacity = '0';
    textarea.value = text;
    document.body.appendChild(textarea);
    textarea.focus();
    textarea.select();
    document.execCommand('copy');
    document.body.removeChild(textarea);
  }
}
