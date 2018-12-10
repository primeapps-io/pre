'use strict';

angular.module('primeapps')

    .constant('taskDate', {
        today: new Date().setHours(23, 59, 59, 0),
        tomorrow: new Date().setHours(47, 59, 59, 0),
        nextWeek: new Date().setHours(191, 59, 59, 0),
        future: new Date('2100-12-12').getTime()
    })

    .constant('entityTypes', {
        user: '11111111-1111-1111-1111-111111111111',
        task: '22222222-2222-2222-2222-222222222222',
        document: '33333333-3333-3333-3333-333333333333',
        note: '44444444-4444-4444-4444-444444444444',
        comment: '55555555-5555-5555-5555-555555555555'
    })

    .constant('component', {
        avatars: 1,
        tasks: 2,
        documents: 4,
        entities: 8,
        users: 16,
        licenses: 32
    })

    .constant('operations', {
        read: 'read',
        modify: 'modify',
        write: 'write',
        remove: 'remove'
    })

    .constant('dataTypes', {
        text_single: {
            name: 'text_single',
            label: {
                en: 'Text (Single Line)',
                tr: 'Yazı (Tek Satır)'
            },
            operators: [
                'is',
                'is_not',
                'contains',
                'not_contain',
                'starts_with',
                'ends_with',
                'empty',
                'not_empty'
            ],
            order: 1
        },
        text_multi: {
            name: 'text_multi',
            label: {
                en: 'Text (Multi Line)',
                tr: 'Yazı (Çok Satır)'
            },
            operators: [
                'is',
                'is_not',
                'contains',
                'not_contain',
                'starts_with',
                'ends_with',
                'empty',
                'not_empty'
            ],
            order: 2
        },
        number: {
            name: 'number',
            label: {
                en: 'Number',
                tr: 'Sayı'
            },
            operators: [
                'equals',
                'not_equal',
                'greater',
                'greater_equal',
                'less',
                'less_equal',
                'empty',
                'not_empty'
            ],
            order: 3
        },
        number_auto: {
            name: 'number_auto',
            label: {
                en: 'Number (Auto)',
                tr: 'Sayı (Otomatik)'
            },
            operators: [
                'equals',
                'not_equal',
                'greater',
                'greater_equal',
                'less',
                'less_equal',
                'empty',
                'not_empty'
            ],
            order: 4
        },
        number_decimal: {
            name: 'number_decimal',
            label: {
                en: 'Number (Decimal)',
                tr: 'Sayı (Ondalık)'
            },
            operators: [
                'equals',
                'not_equal',
                'greater',
                'greater_equal',
                'less',
                'less_equal',
                'empty',
                'not_empty'
            ],
            order: 5
        },
        currency: {
            name: 'currency',
            label: {
                en: 'Currency',
                tr: 'Para'
            },
            operators: [
                'equals',
                'not_equal',
                'greater',
                'greater_equal',
                'less',
                'less_equal',
                'empty',
                'not_empty'
            ],
            order: 6
        },
        date: {
            name: 'date',
            label: {
                en: 'Date',
                tr: 'Tarih'
            },
            operators: [
                'equals',
                'not_equal',
                'greater',
                'greater_equal',
                'less',
                'less_equal',
                'empty',
                'not_empty'
            ],
            order: 7
        },
        date_time: {
            name: 'date_time',
            label: {
                en: 'Date / Time',
                tr: 'Tarih / Saat'
            },
            operators: [
                'equals',
                'not_equal',
                'greater',
                'greater_equal',
                'less',
                'less_equal',
                'empty',
                'not_empty'
            ],
            order: 8
        },
        time: {
            name: 'time',
            label: {
                en: 'Time',
                tr: 'Saat'
            },
            operators: [
                'equals',
                'not_equal',
                'greater',
                'greater_equal',
                'less',
                'less_equal',
                'empty',
                'not_empty'
            ],
            order: 9
        },
        email: {
            name: 'email',
            label: {
                en: 'E-Mail',
                tr: 'E-Posta'
            },
            operators: [
                'is',
                'is_not',
                'contains',
                'not_contain',
                'starts_with',
                'ends_with',
                'empty',
                'not_empty'
            ],
            order: 10
        },
        picklist: {
            name: 'picklist',
            label: {
                en: 'Pick List',
                tr: 'Seçim Listesi'
            },
            operators: [
                'is',
                'is_not',
                'empty',
                'not_empty'
            ],
            order: 11
        },
        multiselect: {
            name: 'multiselect',
            label: {
                en: 'Multi Select',
                tr: 'Çoklu Seçim'
            },
            operators: [
                'contains',
                'not_contain',
                'is',
                'is_not',
                'empty',
                'not_empty'
            ],
            order: 12
        },
        lookup: {
            name: 'lookup',
            label: {
                en: 'Lookup',
                tr: 'Arama'
            },
            operators: [],
            order: 13
        },
        checkbox: {
            name: 'checkbox',
            label: {
                en: 'Check Box',
                tr: 'Onay Kutusu'
            },
            operators: [
                'equals',
                'not_equal'
            ],
            order: 14
        },
        document: {
            name: 'document',
            label: {
                en: 'Document',
                tr: 'Doküman'
            },
            operators: [
                'starts_with',
                'is'
            ],
            order: 15
        },
        url: {
            name: 'url',
            label: {
                en: 'Url',
                tr: 'Url'
            },
            operators: [
                'is',
                'is_not',
                'contains',
                'not_contain',
                'starts_with',
                'ends_with',
                'empty',
                'not_empty'
            ],
            order: 16
        },
        location: {
            name: 'location',
            label: {
                en: 'Location',
                tr: 'Konum'
            },
            operators: [
                'is',
                'is_not',
                'contains',
                'not_contain',
                'starts_with',
                'ends_with',
                'empty',
                'not_empty'
            ],
            order: 17
        },
        image: {
            name: 'image',
            label: {
                en: 'Image',
                tr: 'Resim'
            },
            operators: [
                'is',
                'is_not',
                'contains',
                'not_contain',
                'starts_with',
                'ends_with',
                'empty',
                'not_empty'
            ],
            order: 18
        },
        rating: {
            name: 'rating',
            label: {
                en: 'Rating',
                tr: 'Derecelendirme'
            },
            operators: [
                'equals',
                'not_equal',
                'greater',
                'greater_equal',
                'less',
                'less_equal',
                'empty',
                'not_empty'
            ],
            order: 19
        },
        tag: {
            name: 'tag',
            label: {
                en: 'Tag',
                tr: 'Tag'
            },
            operators: [
                'contains',
                'not_contain',
                'is',
                'is_not',
                'empty',
                'not_empty'
            ],
            order: 20
        }
        // json: {
        //     name: 'json',
        //     label: {
        //         en: 'Json',
        //         tr: 'Json'
        //     },
        //     operators: [
        //         'contains',
        //         'not_contain',
        //         'is',
        //         'is_not',
        //         'empty',
        //         'not_empty'
        //     ],
        //     order: 21
        // }
    })

    .constant('operators', {
        contains: {
            name: 'contains',
            label: {
                en: 'contains',
                tr: 'içerir'
            },
            order: 1
        },
        not_contain: {
            name: 'not_contain',
            label: {
                en: 'doesn\'t contain',
                tr: 'içermez'
            },
            order: 2
        },
        is: {
            name: 'is',
            label: {
                en: 'is',
                tr: 'eşit'
            },
            order: 3
        },
        is_not: {
            name: 'is_not',
            label: {
                en: 'isn\'t',
                tr: 'eşit değil'
            },
            order: 4
        },
        equals: {
            name: 'equals',
            label: {
                en: 'equals',
                tr: 'eşit'
            },
            order: 5
        },
        not_equal: {
            name: 'not_equal',
            label: {
                en: 'doesn\'t equal',
                tr: 'eşit değil'
            },
            order: 6
        },
        starts_with: {
            name: 'starts_with',
            label: {
                en: 'starts with',
                tr: 'ile başlar'
            },
            order: 7
        },
        ends_with: {
            name: 'ends_with',
            label: {
                en: 'ends with',
                tr: 'ile biter'
            },
            order: 8
        },
        empty: {
            name: 'empty',
            label: {
                en: 'empty',
                tr: 'boş'
            },
            order: 9
        },
        not_empty: {
            name: 'not_empty',
            label: {
                en: 'not empty',
                tr: 'boş değil'
            },
            order: 10
        },
        greater: {
            name: 'greater',
            label: {
                en: 'greater',
                tr: 'büyük'
            },
            order: 11
        },
        greater_equal: {
            name: 'greater_equal',
            label: {
                en: 'greater or equal',
                tr: 'büyük eşit'
            },
            order: 12
        },
        less: {
            name: 'less',
            label: {
                en: 'less',
                tr: 'küçük'
            },
            order: 13
        },
        less_equal: {
            name: 'less_equal',
            label: {
                en: 'less or equal',
                tr: 'küçük eşit'
            },
            order: 14
        }
    })

    .constant('activityTypes', [
        {
            type: 1,
            id: 1,
            label: {
                en: 'Task',
                tr: 'Görev'
            },
            system_code: 'task',
            value: 'task',
            order: 1,
            hidden: false
        },
        {
            type: 1,
            id: 2,
            label: {
                en: 'Event',
                tr: 'Etkinlik'
            },
            system_code: 'event',
            value: 'event',
            order: 2,
            hidden: false
        },
        {
            type: 1,
            id: 3,
            label: {
                en: 'Call',
                tr: 'Arama'
            },
            system_code: 'call',
            value: 'call',
            order: 3,
            hidden: false
        }
    ])

    .constant('transactionTypes', [
        {
            type: 1,
            id: 350,
            label: {
                en: 'Collection',
                tr: 'Tahsilat'
            },
            system_code: 'collection',
            value: 'collection',
            order: 1,
            show: true
        },
        {
            type: 2,
            id: 351,
            label: {
                en: 'Payment',
                tr: 'Ödeme'
            },
            system_code: 'payment',
            value: 'payment',
            order: 2,
            show: true
        }
        ,
        {
            type: 1,
            id: 352,
            label: {
                en: 'Sales Invoice',
                tr: 'Satış Faturası'
            },
            system_code: 'sales_invoice',
            value: 'sales_invoice',
            order: 3,
            show: true
        },
        {
            type: 2,
            id: 353,
            label: {
                en: 'Purchase Invoice',
                tr: 'Alış Faturası'
            },
            system_code: 'purchase_invoice',
            value: 'purchase_invoice',
            order: 4,
            show: true
        }
    ])

    .constant('yesNo', [
        {
            type: 2,
            id: 4,
            label: {
                en: 'Yes',
                tr: 'Evet'
            },
            system_code: 'true',
            order: 1
        },
        {
            type: 2,
            id: 5,
            label: {
                en: 'No',
                tr: 'Hayır'
            },
            system_code: 'false',
            order: 2
        }
    ])

    .constant('systemFields', [
        'id',
        'deleted',
        'shared_users',
        'shared_user_groups',
        'shared_users_edit',
        'shared_user_groups_edit',
        'is_sample',
        'is_converted',
        'master_id',
        'migration_id',
        'import_id',
        'created_by',
        'updated_by',
        'created_at',
        'updated_at',
        'currency'
    ])

    .constant('systemRequiredFields', {
        all: [
            'owner',
            'created_by',
            'created_at',
            'updated_by',
            'updated_at'
        ],
        activities: [
            'activity_type',
            'related_module',
            'related_to',
            'subject',
            'task_due_date',
            'task_status',
            'task_notification',
            'task_reminder',
            'reminder_recurrence',
            'event_start_date',
            'event_end_date',
            'event_reminder',
            'call_time'
        ],
        opportunities: [
            'amount',
            'closing_date',
            'stage',
            'probability',
            'expected_revenue'
        ],
        products: [
            'name',
            'unit_price',
            'usage_unit',
            'purchase_price',
            'vat_percent',
            'using_stock',
            'stock_quantity',
            'critical_stock_limit',
            'aktif_urun_hizmet'
        ],
        quotes: [
            'account',
            'quote_stage',
            'total',
            'vat_total',
            'grand_total',
            'discount_amount',
            'discount_percent',
            'discount_type',
            'vat_list',
            'email',
            'currency',
            'discounted_total'
        ],
        sales_orders: [
            'account',
            'order_stage',
            'total',
            'vat_total',
            'grand_total',
            'discount_amount',
            'discount_percent',
            'discount_type',
            'vat_list',
            'email',
            'currency',
            'discounted_total',
            'approved'
        ],
        purchase_orders: [
            'supplier',
            'order_stage',
            'total',
            'vat_total',
            'grand_total',
            'discount_amount',
            'discount_percent',
            'discount_type',
            'vat_list',
            'email',
            'currency',
            'approved',
            'discounted_total'
        ],
        calisanlar: [
            'sabit_devreden_izin',
            'devreden_izin',
            'kalan_izin_hakki',
            'hakedilen_izin'
        ],
        human_resources: [
            'sabit_devreden_izin',
            'devreden_izin',
            'kalan_izin_hakki',
            'hakedilen_izin'
        ],
        izinler: [
            'bitis_tarihi',
            'baslangic_tarihi',
            'from_entry_type',
            'to_entry_type',
            'mevcut_kullanilabilir_izin',
            'hesaplanan_alinacak_toplam_izin',
            'custom_approver',
            'talep_edilen_izin',
            'calisan',
            'izin_turu'
        ],
        izin_turleri: [
            'adi',
            'yillik_izin',
            'yillik_hakedilen_limit_gun',
            'tek_seferde_alinabilecek_en_fazla_izin_gun',
            'izin_hakkindan_dusulsun',
            'yillik_izin_hakki_gun',
            'cuma_gunu_alinan_izinlere_cumartesiyi_de_ekle',
            'izin_hakkindan_takvim_gunu_olarak_dusulsun',
            'resmi_tatiller_ile_birlestirilebilir',
            'izin_borclanma_yapilabilir',
            'saatlik_kullanim_yapilir',
            'saatlik_kullanimi_yukari_yuvarla',
            'dogum_gunu_izni',
            'sonraki_doneme_devredilen_izin_gun',
            'ilk_izin_kullanimi_hakedis_zamani_ay',
            'yasa_gore_asgari_izin_gun',
            'yillik_izine_ek_izin_suresi_ekle',
            'yillik_izine_ek_izin_suresi_gun',
            'ek_izin_sonraki_yillara_devreder',
            'oncelikle_yillik_izin_kullanimi_dusulur',
            'toplam_calisma_saati',
            'dogum_gunu_izni_kullanimi',
            'sadece_tam_gun_olarak_kullanilir'
        ],
        current_accounts: [
            'transaction_type',
            'date',
            'customer',
            'supplier',
            'currency',
            'borc_tl',
            'alacak',
            'bakiye_tl',
            'borc_usd',
            'alacak_usd',
            'bakiye_usd',
            'borc_euro',
            'alacak_euro',
            'bakiye_euro',
            'payment_method'
        ],
        stock_transactions: [
            'transaction_date',
            'stock_transaction_type',
            'quantity',
            'product',
            'sales_order',
            'purchase_order',
            'supplier',
            'customer',
            'cikan_miktar',
            'bakiye'
        ],
        holidays: [
            'date',
            'country'
        ],
        accounts: [
            'aktif_firma'
        ],
        suppliers: [
            'aktif_tedarikci'
        ],
        sales_invoices: [
            'approved',
            'currency',
            'account',
            'fatura_tarihi',
            'grand_total',
            'discount_amount',
            'discount_percent',
            'discount_type',
            'discounted_total',
            'total',
            'vat_list'

        ],
        purchase_invoices: [
            'approved',
            'currency',
            'tedarikci',
            'fatura_tarihi',
            'grand_total',
            'discount_amount',
            'discount_percent',
            'discount_type',
            'discounted_total',
            'total',
            'vat_list'
        ],
        kasalar: [
            'para_birimi',
            'guncel_bakiye',
            'hesap_no',
            'kasa_adi'
        ],
        bankalar: [
            'banka_adi',
            'hesap_no',
            'para_birimi',
            'guncel_bakiye'
        ],
        kasa_hareketleri: [
            'kasa',
            'islem_tarihi',
            'hareket_tipi',
            'borc',
            'alacak',
            'bakiye',
            'ilgili_cari_hareket'
        ],
        banka_hareketleri: [
            'islem_tarihi',
            'hareket_tipi',
            'banka',
            'borc',
            'alacak',
            'bakiye',
            'ilgili_cari_hareket'
        ]

    })

    .constant('systemReadonlyFields', {
        all: [
            'owner',
            'created_by',
            'created_at',
            'updated_by',
            'updated_at'
        ],
        activities: [
            'activity_type',
            'related_module',
            'related_to',
            'task_status'
        ],
        opportunities: [
            'expected_revenue'
        ],
        products: [
            'unit_price',
            'vat_percent',
            'purchase_price',
            'stock_quantity'
        ],
        quotes: [
            'account',
            'quote_stage',
            'total',
            'vat_total',
            'grand_total',
            'discount_amount',
            'discount_percent',
            'discount_type',
            'email'
        ],
        orders: [
            'account',
            'order_stage',
            'total',
            'vat_total',
            'grand_total',
            'discount_amount',
            'discount_percent',
            'discount_type',
            'email'
        ],
        current_accounts: [
            'transaction_type',
            'date',
            'amount',
            'customer',
            'supplier'
        ],
        holidays: [
            'date',
            'country'
        ],
        timetrackers: [
            'week',
            'month',
            'year',
            'timetracker_id',
            'date_range',
            'toplam_saat'
        ],
        timetracker_items: [
            'saat',
            'tarih',
            'izindir',
            'related_timetracker'
        ]
    })

    .value('guidEmpty', '00000000-0000-0000-0000-000000000000')

    .value('emailRegex', /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/);