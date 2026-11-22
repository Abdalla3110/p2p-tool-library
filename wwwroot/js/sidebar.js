/* public/js/sidebar.js — shared dashboard sidebar builder.
   Usage: after including functions-data.js, call
   renderSidebar('admin'|'support', '<activeHref>').
   Unit groups are collapsible; the group containing the active item stays open.
   Pages live in role folders (admin/, library/, maintenance/), so every
   href is resolved relative to the current page location.
*/

/* filename -> folder ('' = public root) */
const SIDEBAR_FILE_FOLDER = {
  'admin-dashboard.html': 'admin', 'admin-settings.html': 'admin',
  'role-management.html': 'admin', 'audit-log.html': 'admin',
  'support-dashboard.html': 'library', 'disputes.html': 'library',
  'dispute-detail.html': 'library', 'inventory-audits.html': 'library',
  'audit-detail.html': 'library', 'handover-management.html': 'library',
  'handover-detail.html': 'library', 'insurance-claims.html': 'library',
  'claim-detail.html': 'library', 'late-returns.html': 'library',
  'blacklist.html': 'library', 'promotions.html': 'library',
  'broadcasts.html': 'library', 'zones.html': 'library', 'reports.html': 'library',
  'bookings.html': 'library', 'users.html': 'library', 'tools-admin.html': 'library',
  'add-user.html': 'library', 'librarian-tasks.html': 'library',
  'maintenance-dashboard.html': 'maintenance', 'external-repair.html': 'maintenance',
  'disposal.html': 'maintenance', 'maintenance-history.html': 'maintenance',
  'technician-metrics.html': 'maintenance', 'technician-profile.html': 'maintenance',
  'tool-health.html': 'maintenance',
  'member-dashboard.html': 'member', 'my-tools.html': 'member',
  'my-bookings.html': 'member', 'booking-detail.html': 'member',
  'checkout.html': 'member', 'wallet.html': 'member', 'notifications.html': 'member',
  'damage-report.html': 'member', 'handover.html': 'member',
  'booking-calendar.html': 'member', 'ratings.html': 'member',
  'edit-tool.html': 'member', 'payment-history.html': 'member',
  'referral.html': 'member', 'add-tool.html': 'member'
};

function sidebarRel(href) {
  const m = href.match(/^([A-Za-z0-9_-]+\.html)([?#].*)?$/);
  if (!m) return href;
  const name = m[1], suffix = m[2] || '';
  const to = SIDEBAR_FILE_FOLDER[name] || '';
  const cm = location.pathname.replace(/\\/g, '/').match(/\/(admin|library|maintenance|member)\//);
  const cur = cm ? cm[1] : '';
  if (cur === to) return name + suffix;
  if (cur === '') return (to ? to + '/' : '') + name + suffix;
  if (to === '') return '../' + name + suffix;
  return '../' + to + '/' + name + suffix;
}

function renderSidebar(role, activeHref) {
  const items = [];
  const item = (href, icon, label, badge) => {
    const h = sidebarRel(href);
    items.push(
      `<a href="${h}" class="sidebar-item${(h === activeHref || href === activeHref) ? ' active' : ''}">` +
      (badge || '') +
      `<i class="fas ${icon}"></i><span class="fn-name">${label}</span></a>`
    );
  };
  const fn = n => sidebarRel('function.html') + '?fn=' + n;

  if (role === 'admin') {
    items.push('<div class="sidebar-header">Main Menu</div>');
    item('admin-dashboard.html', 'fa-chart-line', 'Overview');
    item(fn(28), 'fa-tachometer-alt', 'Real-time Activity Dashboard');
    item('bookings.html', 'fa-calendar-alt', 'Bookings');
    item('tools-admin.html', 'fa-tools', 'Tools Management');
    item('users.html', 'fa-users', 'Users');
    item('librarian-tasks.html', 'fa-list-check', 'My Tasks');
    items.push('<div class="sidebar-header">Library Operations</div>');
    item('disputes.html', 'fa-gavel', 'Disputes');
    item('inventory-audits.html', 'fa-clipboard-check', 'Inventory Audits');
    item('handover-management.html', 'fa-qrcode', 'Handovers');
    item('insurance-claims.html', 'fa-file-invoice', 'Insurance Claims');
    item('late-returns.html', 'fa-clock', 'Late Returns');
    item('blacklist.html', 'fa-ban', 'Blacklist');
    item('promotions.html', 'fa-bullhorn', 'Campaigns');
    item('broadcasts.html', 'fa-rss', 'Broadcasts');
    item('reports.html', 'fa-chart-bar', 'Reports');
    item('zones.html', 'fa-map-marked-alt', 'Zones');
    items.push('<div class="sidebar-header">Maintenance</div>');
    item('maintenance-dashboard.html', 'fa-screwdriver-wrench', 'Maintenance Dashboard');
    item('tool-health.html', 'fa-heart-pulse', 'Tool Health');
    item('external-repair.html', 'fa-external-link-alt', 'External Repair');
    item('disposal.html', 'fa-recycle', 'Disposal');
    item('maintenance-history.html', 'fa-history', 'Maintenance History');
    item('technician-metrics.html', 'fa-chart-line', 'Technician Metrics');
  } else {
    items.push('<div class="sidebar-header">Main Menu</div>');
    item('support-dashboard.html', 'fa-chart-line', 'Overview');
    item('bookings.html', 'fa-calendar-alt', 'Bookings');
    item('tools-admin.html', 'fa-tools', 'Tools Management');
    item('users.html', 'fa-users', 'Users');
    item('librarian-tasks.html', 'fa-list-check', 'My Tasks');
    items.push('<div class="sidebar-header">Library Operations</div>');
    item('disputes.html', 'fa-gavel', 'Disputes');
    item('inventory-audits.html', 'fa-clipboard-check', 'Inventory Audits');
    item('handover-management.html', 'fa-qrcode', 'Handovers');
    item('insurance-claims.html', 'fa-file-invoice', 'Insurance Claims');
    item('late-returns.html', 'fa-clock', 'Late Returns');
    item('blacklist.html', 'fa-ban', 'Blacklist');
    item('promotions.html', 'fa-bullhorn', 'Campaigns');
    item('broadcasts.html', 'fa-rss', 'Broadcasts');
    item('reports.html', 'fa-chart-bar', 'Reports');
    item('zones.html', 'fa-map-marked-alt', 'Zones');
  }

  const units = role === 'admin' ? [1, 2, 3] : [2];
  let groupOpen = 0;
  for (const unit of units) {
    items.push(
      '<div class="sidebar-header sidebar-toggle" role="button" tabindex="0">' +
      `<span>${UNIT_NAMES[unit]} <span class="sidebar-count">(${FUNCTIONS.filter(x => x.unit === unit).length})</span></span>` +
      '<i class="fas fa-chevron-down sidebar-chevron"></i></div>'
    );
    for (const f of FUNCTIONS.filter(x => x.unit === unit)) {
      item(
        fn(f.n),
        f.icon,
        f.name,
        `<span class="fn-num">${String(f.n).padStart(2, '0')}</span>`
      );
    }
    groupOpen++;
  }

  if (role === 'support') {
    items.push('<div class="sidebar-header">Support</div>');
    item('messages.html', 'fa-envelope', 'Messages');
    item('profile.html', 'fa-user', 'My Profile');
  }

  const nav = document.getElementById('sidebar-nav');
  if (!nav) return;
  nav.innerHTML = items.join('');

  /* group each collapsible block: header + following items until next header */
  const headers = [...nav.querySelectorAll('.sidebar-header')];
  headers.forEach(header => {
    if (!header.classList.contains('sidebar-toggle')) return;
    const group = document.createElement('div');
    group.className = 'sidebar-group';
    nav.insertBefore(group, header);
    let node = header.nextElementSibling;
    group.appendChild(header);
    while (node && !node.classList.contains('sidebar-header')) {
      const next = node.nextElementSibling;
      group.appendChild(node);
      node = next;
    }
    /* keep the group with the active link open, collapse the others */
    const hasActive = !!group.querySelector('.sidebar-item.active');
    if (!hasActive) group.classList.add('collapsed');

    const toggle = () => {
      group.classList.toggle('collapsed');
      group.querySelector('.sidebar-chevron')
        .classList.toggle('flipped', !group.classList.contains('collapsed'));
    };
    header.addEventListener('click', toggle);
    header.addEventListener('keydown', e => {
      if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); toggle(); }
    });
    const chev = group.querySelector('.sidebar-chevron');
    if (chev && !group.classList.contains('collapsed')) chev.classList.add('flipped');
  });
}
