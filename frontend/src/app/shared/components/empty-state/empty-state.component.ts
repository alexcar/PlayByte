import { Component, Input } from '@angular/core';

@Component({
  selector: 'pb-empty-state',
  standalone: true,
  template: `
    <div class="pb-empty">
      <i [class]="'pi ' + icon"></i>
      <h4>{{ title }}</h4>
      <p>{{ message }}</p>
    </div>
  `,
  styles: [
    `
      .pb-empty {
        text-align: center;
        padding: 4rem 1rem;
        color: var(--pb-muted);
      }
      i {
        font-size: 3rem;
        display: block;
        margin-bottom: 1rem;
      }
      h4 {
        font-family: var(--pb-font-display);
        font-weight: 700;
        margin-bottom: 0.5rem;
        color: var(--pb-text);
      }
      p {
        font-size: 0.9rem;
        margin: 0;
      }
    `,
  ],
})
export class EmptyStateComponent {
  @Input() icon = 'pi-inbox';
  @Input() title = 'Nada por aqui';
  @Input() message = '';
}
