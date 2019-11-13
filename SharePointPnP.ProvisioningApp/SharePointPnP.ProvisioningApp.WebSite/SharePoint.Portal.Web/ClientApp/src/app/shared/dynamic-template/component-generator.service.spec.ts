import { Component } from '@angular/core';
import { TestBed, ComponentFixture, async } from '@angular/core/testing';
import { CommonModule } from '@angular/common';

import { ComponentGeneratorService } from './component-generator.service';
import { By } from '@angular/platform-browser';

@Component({
    template: `
    <ng-template [ngIf]="component && module">
        <ng-container *ngComponentOutlet="component; ngModuleFactory: module"></ng-container>
    </ng-template>
    `
})
class ComponentGeneratorTestComponent {
    component: any;
    module: any;

    count = 0;

    testFunction() {
        this.count++;
    }
}

describe('ComponentGeneratorService', () => {
    let service: ComponentGeneratorService;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            imports: [CommonModule],
            declarations: [ComponentGeneratorTestComponent]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        service = TestBed.get(ComponentGeneratorService);
    });

    describe('basic tests', () => {
        it('should be created', () => {
            expect(service).toBeTruthy();
        });

        it('should generate a component', () => {
            const generatedComponent = service.createComponent({ html: '<span>test</span>' });
            expect(generatedComponent).toBeTruthy();
            expect(typeof generatedComponent).toEqual('function');
        });

        it('should generate a module', async(() => {
            const generatedComponent = service.createComponent({ html: '<span>test</span>' });
            const generatedModule = service.createModuleForComponent(generatedComponent, {});
            expect(generatedModule).toBeTruthy();

            expect(generatedModule.create).toBeTruthy();
        }));

        it('should generate multiple module, reusing the same definition object', async(() => {
            const moduleDefn = {};

            const generatedComponent1 = service.createComponent({ html: '<span>test</span>' });
            const generatedModule1 = service.createModuleForComponent(generatedComponent1, moduleDefn);
            expect(generatedModule1).toBeTruthy();

            expect(generatedModule1.create).toBeTruthy();

            const generatedComponent2 = service.createComponent({ html: '<span>test</span>' });
            const generatedModule2 = service.createModuleForComponent(generatedComponent2, moduleDefn);
            expect(generatedModule2).toBeTruthy();

            expect(generatedModule2.create).toBeTruthy();

            expect(generatedModule1).not.toBe(generatedModule2);
        }));
    });

    describe('component rendering', () => {
        let fixture: ComponentFixture<ComponentGeneratorTestComponent>;
        let component: ComponentGeneratorTestComponent;

        beforeEach(() => {
            fixture = TestBed.createComponent(ComponentGeneratorTestComponent);
            component = fixture.componentInstance;
        });

        it('should give the html', () => {
            const generatedComponent = service.createComponent({ html: '<span>test</span>' });
            const generatedModule = service.createModuleForComponent(generatedComponent, {});

            component.component = generatedComponent;
            component.module = generatedModule;
            fixture.detectChanges();

            const elem = fixture.debugElement.query(By.css('span'));
            expect(elem).toBeTruthy();

            expect(fixture.debugElement.nativeElement.innerText).toEqual('test');
        });

        it('should apply the style', () => {
            const generatedComponent = service.createComponent({
                html: '<span>test</span>',
                css: 'span { color: #ff00ff; font-weight: 600 }'
            });
            const generatedModule = service.createModuleForComponent(generatedComponent, {});

            component.component = generatedComponent;
            component.module = generatedModule;
            fixture.detectChanges();

            const elem = fixture.debugElement.query(By.css('span')).nativeElement as HTMLElement;
            const styles = window.getComputedStyle(elem);
            expect(styles.color).toEqual('rgb(255, 0, 255)');
            expect(styles.fontWeight).toEqual('600');
        });

        it('should bind the provided data', () => {
            const generatedComponent = service.createComponent({
                html: '<span>{{ context.text }}</span>',
            });
            const generatedModule = service.createModuleForComponent(generatedComponent, {}, { text: 'bound text'});

            component.component = generatedComponent;
            component.module = generatedModule;
            fixture.detectChanges();

            const elem = fixture.debugElement.query(By.css('span'));
            expect(elem).toBeTruthy();
            expect(fixture.debugElement.nativeElement.innerText).toEqual('bound text');
        });

        it('should be able to call functions in the parent component', () => {
            const spy = spyOn(component, 'testFunction').and.callThrough();
            const generatedComponent = service.createComponent({
                html: '<span (click)="context.testFunction()">text</span>',
            });
            const generatedModule = service.createModuleForComponent(generatedComponent, {}, component);

            component.component = generatedComponent;
            component.module = generatedModule;
            fixture.detectChanges();

            const elem = fixture.debugElement.query(By.css('span'));

            elem.triggerEventHandler('click', {});
            expect(spy).toHaveBeenCalledTimes(1);
            expect(component.count).toBe(1);

            elem.triggerEventHandler('click', {});
            expect(spy).toHaveBeenCalledTimes(2);
            expect(component.count).toBe(2);
        });
    });
});
